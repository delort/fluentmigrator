﻿#region License
// Copyright (c) 2018, FluentMigrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// The default implementation of <see cref="IProfileSource"/>
    /// </summary>
    public class ProfileSource : IProfileSource
    {
        [NotNull]
        private readonly IAssemblySource _source;

        [NotNull]
        private readonly IMigrationRunnerConventions _conventions;

        [NotNull]
        private readonly IServiceProvider _serviceProvider;

        [NotNull]
        private readonly ConcurrentDictionary<Type, IMigration> _instanceCache = new ConcurrentDictionary<Type, IMigration>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileSource"/> class.
        /// </summary>
        /// <param name="source">The assembly source</param>
        /// <param name="conventions">The migration runner conventios</param>
        /// <param name="serviceProvider">The service provider</param>
        public ProfileSource(
            [NotNull] IAssemblySource source,
            [NotNull] IMigrationRunnerConventions conventions,
            [NotNull] IServiceProvider serviceProvider)
        {
            _source = source;
            _conventions = conventions;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public IEnumerable<IMigration> GetProfiles(string profile)
        {
            var instances = from type in _source.Assemblies.SelectMany(a => a.GetExportedTypes())
                            where _conventions.TypeIsProfile(type)
                            let profileAttribute = type.GetCustomAttribute<ProfileAttribute>()
                            where string.IsNullOrEmpty(profile) || string.Equals(profileAttribute.ProfileName, profile)
                            select _instanceCache.GetOrAdd(type, t => (IMigration)ActivatorUtilities.CreateInstance(_serviceProvider, t));
            return instances;
        }
    }
}
