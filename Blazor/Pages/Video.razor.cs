using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop;
using Syncfusion.Blazor.PivotView;

namespace SpotOn.Web.Pages
{
    public class VideoBase : ComponentBase
    {
        [Inject] 
        protected IJSRuntime JsRuntime { get; set; }

        protected override async Task OnAfterRenderAsync(bool FirstRender)
        {
            if (FirstRender)
            {
                await JsRuntime.InvokeVoidAsync("Video");
            }
        }
        [JSInvokable]
        public static void Hello()
        {

        }


    }
}
