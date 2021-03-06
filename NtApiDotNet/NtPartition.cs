﻿//  Copyright 2018 Google Inc. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System;
using System.Runtime.InteropServices;

namespace NtApiDotNet
{
#pragma warning disable 1591
    [Flags]
    public enum MemoryPartitionAccessRights : uint
    {
        None = 0,
        Query = 1,
        Modify = 2,
        GenericRead = GenericAccessRights.GenericRead,
        GenericWrite = GenericAccessRights.GenericWrite,
        GenericExecute = GenericAccessRights.GenericExecute,
        GenericAll = GenericAccessRights.GenericAll,
        Delete = GenericAccessRights.Delete,
        ReadControl = GenericAccessRights.ReadControl,
        WriteDac = GenericAccessRights.WriteDac,
        WriteOwner = GenericAccessRights.WriteOwner,
        Synchronize = GenericAccessRights.Synchronize,
        MaximumAllowed = GenericAccessRights.MaximumAllowed,
        AccessSystemSecurity = GenericAccessRights.AccessSystemSecurity
    }

    public enum MemoryPartitionInformationClass
    {
        SystemMemoryPartitionInformation,
        SystemMemoryPartitionMoveMemory,
        SystemMemoryPartitionAddPagefile,
        SystemMemoryPartitionCombineMemory,
        SystemMemoryPartitionInitialAddMemory,
        SystemMemoryPartitionGetMemoryEvents
    }

    public static partial class NtSystemCalls
    {
        [DllImport("ntdll.dll")]
        public static extern NtStatus NtCreatePartition(
            SafeKernelObjectHandle ParentPartitionHandle,
            out SafeKernelObjectHandle PartitionHandle,
            AccessMask DesiredAccess,
            [In] ObjectAttributes ObjectAttributes,
            int PreferredNode
            );

        [DllImport("ntdll.dll")]
        public static extern NtStatus NtOpenPartition(
            out SafeKernelObjectHandle PartitionHandle,
            AccessMask DesiredAccess,
            [In] ObjectAttributes ObjectAttributes
        );

        [DllImport("ntdll.dll")]
        public static extern NtStatus NtManagePartition(
            MemoryPartitionInformationClass PartitionInformationClass,
            SafeBuffer PartitionInformation,
            int PartitionInformationLength
            );
    }
#pragma warning restore 1591

    /// <summary>
    /// Class representing a NT Partition object
    /// </summary>
    [NtType("Partition")]
    public class NtPartition : NtObjectWithDuplicate<NtPartition, MemoryPartitionAccessRights>
    {
        internal NtPartition(SafeKernelObjectHandle handle) : base(handle)
        {
        }

        /// <summary>
        /// Create a partition object
        /// </summary>
        /// <param name="object_attributes">The object attributes</param>
        /// <param name="parent_partition">Optional parent parition.</param>
        /// <param name="desired_access">Desired access for the partition.</param>
        /// <param name="preferred_node">The preferred node, -1 for any node.</param>
        /// <param name="throw_on_error">True to throw an exception on error.</param>
        /// <returns>The NT status code and object result.</returns>
        public static NtResult<NtPartition> Create(ObjectAttributes object_attributes, MemoryPartitionAccessRights desired_access, NtPartition parent_partition, int preferred_node, bool throw_on_error)
        {
            return NtSystemCalls.NtCreatePartition(parent_partition.GetHandle(),
                out SafeKernelObjectHandle handle, desired_access, object_attributes, preferred_node).CreateResult(throw_on_error, () => new NtPartition(handle));
        }

        /// <summary>
        /// Create a partition object
        /// </summary>
        /// <param name="object_attributes">The object attributes</param>
        /// <param name="parent_partition">Optional parent parition.</param>
        /// <param name="desired_access">Desired access for the partition.</param>
        /// <param name="preferred_node">The preferred node, -1 for any node.</param>
        /// <returns>The NT status code and object result.</returns>
        public static NtPartition Create(ObjectAttributes object_attributes, MemoryPartitionAccessRights desired_access, NtPartition parent_partition, int preferred_node)
        {
            return Create(object_attributes, desired_access, parent_partition, preferred_node, true).Result;
        }

        /// <summary>
        /// Open a partition object
        /// </summary>
        /// <param name="object_attributes">The object attributes</param>
        /// <param name="desired_access">Desired access for the partition.</param>
        /// <param name="throw_on_error">True to throw an exception on error.</param>
        /// <returns>The NT status code and object result.</returns>
        public static NtResult<NtPartition> Open(ObjectAttributes object_attributes, MemoryPartitionAccessRights desired_access, bool throw_on_error)
        {
            return NtSystemCalls.NtOpenPartition(out SafeKernelObjectHandle handle, desired_access, object_attributes).CreateResult(throw_on_error, () => new NtPartition(handle));
        }

        /// <summary>
        /// Open a partition object
        /// </summary>
        /// <param name="object_attributes">The object attributes</param>
        /// <param name="desired_access">Desired access for the partition.</param>
        /// <returns>The NT status code and object result.</returns>
        public static NtPartition Open(ObjectAttributes object_attributes, MemoryPartitionAccessRights desired_access)
        {
            return Open(object_attributes, desired_access, true).Result;
        }

        internal static NtResult<NtObject> FromName(ObjectAttributes object_attributes, AccessMask desired_access, bool throw_on_error)
        {
            return Open(object_attributes, desired_access.ToSpecificAccess<MemoryPartitionAccessRights>(), throw_on_error).Cast<NtObject>();
        }
    }
}