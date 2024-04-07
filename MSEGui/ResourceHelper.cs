using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MSEGui
{
    internal static class ResourceHelper
    {
        public static string GetResourceString(this Window window, string resourceName)
        {
            return window.TryFindResource(resourceName) as string;
        }

    }
}
