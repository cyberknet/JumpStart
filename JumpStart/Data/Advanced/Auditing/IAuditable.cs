using System;
using System.Collections.Generic;
using System.Text;

namespace JumpStart.Data.Advanced.Auditing;

/// <summary>
/// Defines the contract for entities that track complete audit information including creation, modification, and deletion.
/// This interface combines <see cref="ICreatable{T}"/>, <see cref="IModifiable{T}"/>, and <see cref="IDeletable{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the user identifier. Must be a reference type.</typeparam>
public interface IAuditable<T> 
    : ICreatable<T>, IModifiable<T>, IDeletable<T>
    where T : struct
{

}
