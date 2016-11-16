﻿namespace Edubase.Data.Identity
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string LA = "LA";
        public const string Academy = "Academy";

        public static readonly string[] RestrictiveRoles = new string[] { LA, Academy };
    }
}