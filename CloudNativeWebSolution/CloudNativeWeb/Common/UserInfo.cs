using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CloudNativeWeb.Common
{
    public class UserInfo
    {
        private static object m_DoubleLock = new object();
        private static UserInfo m_Current = null;
        private static ClaimsIdentity m_Claims = null;

        public static UserInfo Current
        {
            get
            {
                if (null == m_Current)
                {
                    lock (m_DoubleLock)
                    {
                        if (null == m_Current)
                        {
                            m_Current = new UserInfo();
                        }
                    }
                }
                return m_Current;
            }
        }

        public UserInfo()
        {

        }

        public void SetIdentity(ClaimsIdentity identity)
        {
            m_Claims = identity;
        }

        public string ID
        {
            get
            {
                if (m_Claims != null)
                {
                    return m_Claims.FindFirst("UserId").Value;
                }
                else
                {
                    return null;
                }
            }
        }

        public string DisplayName
        {
            get
            {
                if (m_Claims != null)
                {
                    return m_Claims.FindFirst("UserName").Value;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
