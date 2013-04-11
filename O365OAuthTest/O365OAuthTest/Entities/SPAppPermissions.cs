using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace O365OAuthTest.Entities
{
    public class ScopeAlias
    {
        public string Name { get; set; }
        public ScopeAlias(string name)
        {
            Name = name;
        }
        public static readonly ScopeAlias Site = new ScopeAlias("Site");
        public static readonly ScopeAlias Web = new ScopeAlias("Web");
        public static readonly ScopeAlias List = new ScopeAlias("List");
        public static readonly ScopeAlias AllProfiles = new ScopeAlias("AllProfiles");
    }

    public class Rights
    {
        public string Name { get; set; }
        public Rights(string name)
        {
            Name = name;
        }
        public static readonly Rights Read = new Rights("Read");
        public static readonly Rights Write = new Rights("Write");
        public static readonly Rights Manage = new Rights("Manage");
    }
}