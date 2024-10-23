using System;
using System.Collections.Generic;
using IIB.ICDD.Model;
using IIB.ICDD.Validation;

namespace IcddWebApp.WebApplication.Environment
{
    /// <summary>
    /// A static class delivering methods for Session Handling.
    /// </summary>
    public static class SessionManager
    {
        public static Dictionary<string, Session> SessionDict = new Dictionary<string, Session>();
        /// <summary>
        /// Starts a new <c>Session</c> with a specified <c>IcddContainer</c> file.
        /// </summary>
        /// <param name="container">The container file for which the session is started.</param>
        /// <returns>a <c>string</c> containing the GUID of the <c>Session</c></returns>
        public static string StartSession(InformationContainer container)
        {
            var session = new Session(container);
            SessionDict.Add(session.GetGuid(), session);
            Logger.Log("A new session for the Container " + container.ContainerName + " has been started with the GUID '" + session.GetGuid() + "'.", Logger.MsgType.Info, "IcddValidator.IsValid()");
            return session.GetGuid();
        }
        public static bool CloseSession(string sessionGuid)
        {
            Session session;
            if (SessionDict.TryGetValue(sessionGuid, out session) && session.IsAlive())
            {
                SessionDict.Remove(sessionGuid);
                session.Dispose();
                return true;
            }
            return false;
        }

        public static Session GetSession(string sessionGuid)
        {
            if (sessionGuid == null)
                return null;

            Session session;
            if (SessionDict.TryGetValue(sessionGuid, out session) && session.IsAlive())
            {
                return session;
            }
            SessionDict.Remove(sessionGuid);
            return null;
        }

        public static bool UpdateSession(string sessionGuid, InformationContainer container)
        {
            Session session;
            if (SessionDict.TryGetValue(sessionGuid, out session) && session.IsAlive())
            {
                
                return session.SetContainer(container); ;
            }
            return false;
        }
    }

    /// <summary>
    /// A class delivering a Session object.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class Session : IDisposable
    {
        /// <summary>
        /// The session unique identifier
        /// </summary>
        public string SessionGuid;
        public DateTime LastActivity;
        public InformationContainer ConnectedContainer;
        public List<IcddValidationResult> ValidationResults;

        /// <summary>
        /// Initializes a new instance of the <see cref="Session"/> class.
        /// </summary>
        /// <param name="container">The <c>IcddContainer</c> file.</param>
        public Session(InformationContainer container)
        {
            ConnectedContainer = container;
            LastActivity = DateTime.Now;
            SessionGuid = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            ConnectedContainer.Delete();
            ConnectedContainer = null;
        }

        /// <summary>
        /// Gets the unique identifier.
        /// </summary>
        /// <returns></returns>
        public string GetGuid()
        {
            return SessionGuid;
        }
        /// <summary>
        /// Gets the container.
        /// </summary>
        /// <returns>the respective <c>IcddContainer</c> file</returns>
        public InformationContainer GetContainer()
        {
            return IsAlive() ? ConnectedContainer : null;
        }
        public bool SetContainer(InformationContainer container)
        {
            if (!IsAlive()) return false;
            ConnectedContainer = container;
            return true;
        }

        public List<IcddValidationResult> GetValidationResults()
        {
            return IsAlive() ? ValidationResults : null;
        }
        public int GetValidationErrors() {

            int nErrors = 0;
            foreach (var res in ValidationResults)
            {
                if (res.ValidationResult())
                    continue;
                nErrors++;
            }
            return nErrors;
        }
        public bool SetValidationResults(List<IcddValidationResult> results)
        {
            if (!IsAlive()) return false;
            ValidationResults = results;
            return true;
        }

        public bool IsAlive()
        {
            if (DateTime.Now - LastActivity > new TimeSpan(0, 30, 0))
            {
                Dispose();
                return false;
            }
            LastActivity = DateTime.Now;
            return true;
        }
    }
}
