using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace SqCopyResolution.Model
{
    public class SqCopyResolutionSection : ConfigurationSection
    {
        // Create a "font" element.
        [ConfigurationProperty("profiles")]
        public ProfilesCollection Profiles
        {
            get
            {
                return (ProfilesCollection)this["profiles"];
            }
        }
    }

    [ConfigurationCollection(typeof(ProfileElement), AddItemName = "profile")]
    public class ProfilesCollection : ConfigurationElementCollection, IEnumerable<ProfileElement>
    {

        protected override ConfigurationElement CreateNewElement()
        {
            return new ProfileElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var l_configElement = element as ProfileElement;
            if (l_configElement != null)
                return l_configElement.Name;
            else
                return null;
        }

        public ProfileElement this[int index]
        {
            get
            {
                return BaseGet(index) as ProfileElement;
            }
        }

        public new ProfileElement this[string name]
        {
            get
            {
                return BaseGet(name) as ProfileElement;
            }
        }

        #region IEnumerable<ConfigElement> Members

        IEnumerator<ProfileElement> IEnumerable<ProfileElement>.GetEnumerator()
        {
            return (from i in Enumerable.Range(0, this.Count)
                select this[i])
                .GetEnumerator();
        }

        #endregion
    }

    public class ProfileElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public String Name
        {
            get
            {
                return (String)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }
        [ConfigurationProperty("sonarQube", IsRequired = true)]
        public SonarQubeElement SonarQube
        {
            get
            {
                return (SonarQubeElement)this["sonarQube"];
            }
            set
            {
                this["sonarQube"] = value;
            }
        }
        [ConfigurationProperty("source", IsRequired = true)]
        public SourceElement Source
        {
            get
            {
                return (SourceElement)this["source"];
            }
            set
            {
                this["source"] = value;
            }
        }
        [ConfigurationProperty("destination", IsRequired = true)]
        public DestinationElement Destination
        {
            get
            {
                return (DestinationElement)this["destination"];
            }
            set
            {
                this["destination"] = value;
            }
        }
        [ConfigurationProperty("operation", IsRequired = true)]
        public OperationElement Operation
        {
            get
            {
                return (OperationElement)this["operation"];
            }
            set
            {
                this["operation"] = value;
            }
        }
    }

    public class SourceElement : ConfigurationElement
    {
        [ConfigurationProperty("projectKey", IsRequired = true)]
        public String ProjectKey
        {
            get
            {
                return (String)this["projectKey"];
            }
            set
            {
                this["projectKey"] = value;
            }
        }
    }

    public class DestinationElement : ConfigurationElement
    {
        [ConfigurationProperty("projectKeys", IsRequired = true)]
        public String ProjectKeys
        {
            get
            {
                return (String)this["projectKeys"];
            }
            set
            {
                this["projectKeys"] = value;
            }
        }
    }

    public class SonarQubeElement : ConfigurationElement
    {
        [ConfigurationProperty("url", IsRequired = true)]
        public String Url
        {
            get
            {
                return (String)this["url"];
            }
            set
            {
                this["url"] = value;
            }
        }

        [ConfigurationProperty("userName", IsRequired = true)]
        public String UserName
        {
            get
            {
                return (String)this["userName"];
            }
            set
            {
                this["userName"] = value;
            }
        }

        [ConfigurationProperty("password", IsRequired = true)]
        public String Password
        {
            get
            {
                return (String)this["password"];
            }
            set
            {
                this["password"] = value;
            }
        }
    }

    public class OperationElement : ConfigurationElement
    {
        [ConfigurationProperty("type", IsRequired = true)]
        public String OperationType
        {
            get
            {
                return (String)this["type"];
            }
            set
            {
                this["type"] = value;
            }
        }
    }
}
