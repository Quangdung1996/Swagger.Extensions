using Microsoft.AspNetCore.Mvc;
using System;

namespace QD.Swagger.Extensions
{
    public class CustomApiVersion : ApiVersion
    {
        public CustomApiVersion(ApiVersion version, string groupName) : this(version.MajorVersion ?? 0, version.MinorVersion ?? 0, groupName, version.Status)
        {
        }

        public CustomApiVersion(int majorVersion, int minorVersion, string groupName, string status) : base(majorVersion, minorVersion, status)
        {
            GroupName = groupName;
        }

        public string GroupName { get; }

        public override bool Equals(object obj)
        {
            return base.Equals(obj) && obj is CustomApiVersion version && version.GroupName == GroupName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), GroupName?.GetHashCode());
        }
    }
}