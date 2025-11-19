using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTales.Service.Extensions
{
    public static class IJSRuntimeExtensionscs
    {
        public static async Task ToastrSuccess(this IJSRuntime js, string message)
        {
            await js.InvokeVoidAsync("showToastr", "success", message);
        }

        public static async Task ToastrError(this IJSRuntime js, string message)
        {
            await js.InvokeVoidAsync("showToastr", "error", message);
        }

        public static async Task ToastrInfo(this IJSRuntime js, string message)
        {
            await js.InvokeVoidAsync("showToastr", "info", message);
        }

        public static async Task ToastrWarning(this IJSRuntime js, string message)
        {
            await js.InvokeVoidAsync("showToastr", "warning", message);
        }
    }
}
