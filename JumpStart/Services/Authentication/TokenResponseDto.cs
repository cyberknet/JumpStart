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

namespace JumpStart.Services.Authentication;

/// <summary>
/// Response returned by <see cref="Controllers.TokenController"/>'s token-exchange endpoint. See ADR-013.
/// </summary>
public class TokenResponseDto
{
    /// <summary>Gets or sets the real, permission-resolved JWT.</summary>
    public string Token { get; set; } = string.Empty;
}
