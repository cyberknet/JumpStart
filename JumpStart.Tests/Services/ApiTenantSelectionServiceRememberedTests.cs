// Copyright ©2026 Scott Blomfield
/*
 *  This program is free software: you can redistribute it and/or modify it under the terms of the
 *  GNU General Public License as published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
 *  even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along with this program. If not,
 *  see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JumpStart.MultiTenant.Clients;
using JumpStart.MultiTenant.DTOs;
using JumpStart.Services;
using JumpStart.Services.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Xunit;

namespace JumpStart.Tests.Services;

/// <summary>
/// The bug this exists for: switch to a second organization, navigate anywhere, press F5 - and land back on whichever organization sorts first,
/// because the only thing that carried the choice across a reload was a URL parameter that the first navigation throws away. The choice is now
/// also remembered in the browser, used only while the person still belongs to that organization, and never allowed to fail anything.
/// </summary>
public class ApiTenantSelectionServiceRememberedTests
{
    private sealed class FakeBrowser : IJSRuntime
    {
        public Dictionary<string, string> Storage { get; } = [];
        public Exception? Throws { get; set; }
        public bool Hangs { get; set; }

        public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            await InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (Throws is not null)
            {
                throw Throws;
            }

            if (Hangs)
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }

            switch (identifier)
            {
                case "localStorage.getItem":
                    return (TValue)(object?)(Storage.TryGetValue((string)args![0]!, out var value) ? value : null)!;
                case "localStorage.setItem":
                    Storage[(string)args![0]!] = (string)args[1]!;
                    return default!;
                case "localStorage.removeItem":
                    Storage.Remove((string)args![0]!);
                    return default!;
                default:
                    throw new InvalidOperationException(identifier);
            }
        }
    }

    private sealed class FakeNavigation : NavigationManager
    {
        public FakeNavigation(string uri) => Initialize("https://panel.test/", uri);
        protected override void NavigateToCore(string uri, NavigationOptions options) { }
    }

    private static readonly TenantDto DevAdmin = new() { Id = Guid.NewGuid(), Name = "dev-admin", IsActive = true };
    private static readonly TenantDto Sheltered = new() { Id = Guid.NewGuid(), Name = "Sheltered Gaming", IsActive = true };

    private readonly FakeBrowser _browser = new();

    // A fresh service and accessor and cache each time, as a reload gives: nothing survives it except the browser's own storage.
    private ApiTenantSelectionService NewCircuit(string url = "https://panel.test/", string circuit = "c1", TenantSelectionOptions? options = null)
    {
        var tenants = new Mock<ITenantsApiClient>();
        tenants.Setup(c => c.GetMineAsync()).ReturnsAsync(new List<TenantDto> { Sheltered, DevAdmin });      // dev-admin sorts first
        var services = new ServiceCollection().AddSingleton<IJSRuntime>(_browser).AddSingleton<NavigationManager>(new FakeNavigation(url)).BuildServiceProvider();
        var accessor = new CircuitServicesAccessor { CircuitId = circuit, Services = services };
        return new ApiTenantSelectionService(tenants.Object, Mock.Of<ITokenStore>(), accessor, new CircuitTenantCache(), options ?? new TenantSelectionOptions());
    }

    [Fact]
    public async Task ASwitchIsStillInEffectAfterANavigationAndAReloadWithNoUrlParameter()
    {
        var beforeReload = NewCircuit();
        Assert.Equal(DevAdmin.Id, await beforeReload.GetCurrentTenantIdAsync());          // the default: whichever sorts first
        await beforeReload.SetCurrentTenantAsync(Sheltered.Id);

        var afterF5 = NewCircuit(url: "https://panel.test/servers");                      // no tenantId in the address

        Assert.Equal(Sheltered.Id, await afterF5.GetCurrentTenantIdAsync());
    }

    [Fact]
    public async Task TheOrganizationTheUrlAsksForIsRememberedToo()
    {
        var reloadedBySwitcher = NewCircuit(url: $"https://panel.test/?tenantId={Sheltered.Id}");
        Assert.Equal(Sheltered.Id, await reloadedBySwitcher.GetCurrentTenantIdAsync());

        var laterF5 = NewCircuit(url: "https://panel.test/players", circuit: "c2");

        Assert.Equal(Sheltered.Id, await laterF5.GetCurrentTenantIdAsync());
    }

    [Fact]
    public async Task AnExplicitUrlParameterStillBeatsWhatIsRemembered()
    {
        _browser.Storage[ApiTenantSelectionService.StorageKey] = Sheltered.Id.ToString();

        var service = NewCircuit(url: $"https://panel.test/?tenantId={DevAdmin.Id}");

        Assert.Equal(DevAdmin.Id, await service.GetCurrentTenantIdAsync());
    }

    [Fact]
    public async Task WithNothingRememberedTheFirstOrganizationIsTheDefaultAsBefore()
    {
        Assert.Equal(DevAdmin.Id, await NewCircuit().GetCurrentTenantIdAsync());
        Assert.Empty(_browser.Storage);                                                  // resolving the default does not pin it
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task ARememberedValueThatIsNotAnOrganizationOfTheirsIsIgnored(string remembered)
    {
        _browser.Storage[ApiTenantSelectionService.StorageKey] = remembered;

        Assert.Equal(DevAdmin.Id, await NewCircuit().GetCurrentTenantIdAsync());
    }

    [Fact]
    public async Task AnOrganizationSomeoneElseChoseInThisBrowserGrantsNothing()
    {
        var someoneElsesOrganization = Guid.NewGuid();
        _browser.Storage[ApiTenantSelectionService.StorageKey] = someoneElsesOrganization.ToString();

        Assert.Equal(DevAdmin.Id, await NewCircuit().GetCurrentTenantIdAsync());
    }

    [Fact]
    public async Task AnAdministratorActingAsACustomerIsNeverRemembered()
    {
        var customer = Guid.NewGuid();

        var service = NewCircuit(url: $"https://panel.test/?tenantId={customer}", options: new TenantSelectionOptions { AllowCrossTenantSelection = true });

        Assert.Equal(customer, await service.GetCurrentTenantIdAsync());
        Assert.Empty(_browser.Storage);
    }

    [Fact]
    public async Task ABrowserThatCannotBeReachedOrRefusesStorageChangesNothingAndBreaksNothing()
    {
        _browser.Throws = new InvalidOperationException("JavaScript interop calls cannot be issued at this time");
        var service = NewCircuit();

        Assert.Equal(DevAdmin.Id, await service.GetCurrentTenantIdAsync());
        Assert.True(await service.SetCurrentTenantAsync(Sheltered.Id));                  // the switch itself still succeeds

        _browser.Throws = new JSException("SecurityError: storage is blocked");
        Assert.Equal(DevAdmin.Id, await NewCircuit().GetCurrentTenantIdAsync());
    }

    [Fact]
    public async Task ABrowserThatNeverAnswersDoesNotHoldUpTheCircuitForever()
    {
        _browser.Hangs = true;

        var resolved = NewCircuit().GetCurrentTenantIdAsync();
        var finished = await Task.WhenAny(resolved, Task.Delay(TimeSpan.FromSeconds(6)));

        Assert.Same(resolved, finished);
        Assert.Equal(DevAdmin.Id, await resolved);
    }

    [Fact]
    public async Task SigningOutForgetsTheOrganization()
    {
        var service = NewCircuit();
        await service.SetCurrentTenantAsync(Sheltered.Id);
        Assert.NotEmpty(_browser.Storage);

        await service.ClearCurrentTenantAsync();

        Assert.Empty(_browser.Storage);
    }
}
