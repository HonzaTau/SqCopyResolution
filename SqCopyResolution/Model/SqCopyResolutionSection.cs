using System;
using System.Configuration;

namespace SqCopyResolution.Model
{
    public class SqCopyResolutionSection : ConfigurationSection
    {
        // Create a "font" element.
        [ConfigurationProperty("profile")]
        public ProfileElement Profile
        {
            get
            {
                return (ProfileElement)this["profile"];
            }
            set
            { this["profile"] = value; }
        }
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
}
