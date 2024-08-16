using System;
using Autodesk.Revit.UI;

namespace Rvt.Background
{
    /// <summary>
    /// This allows you to execute arbitrary Revit code within a valid UI context. 
    /// </summary>
    public static class ApiContext
    {
        public class ExternalEventHandler : IExternalEventHandler
        {
            public readonly Action<UIApplication> Action;
            public readonly string Name;

            public ExternalEventHandler(Action<UIApplication> action, string name)
            {
                Action = action ?? throw new ArgumentNullException(nameof(action));
                Name = name;
            }

            public void Execute(UIApplication app)
                => Action(app);

            public string GetName()
                => Name;
        }

        public static ExternalEvent CreateEvent(Action<UIApplication> action, string name)
        {
            var eeh = new ExternalEventHandler(action, name);
            return ExternalEvent.Create(eeh);
        }
    }
}