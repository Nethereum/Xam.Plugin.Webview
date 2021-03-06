using Android.Webkit;
using Xam.Plugin.Abstractions.Events.Inbound;
using Xam.Plugin.Abstractions;
using Xam.Plugin.Abstractions.Events.Outbound;
using Android.Graphics;

namespace Xam.Plugin.Droid.Extras
{
    public class FormsWebViewClient : WebViewClient
    {

        private FormsWebView Element { get; set; }
        private FormsWebViewRenderer Renderer { get; set; }

        public FormsWebViewClient(FormsWebView element, FormsWebViewRenderer renderer)
        {
            Element = element;
            Renderer = renderer;
        }

        public override void OnReceivedHttpError(Android.Webkit.WebView view, IWebResourceRequest request, WebResourceResponse errorResponse)
        {
            Element.InvokeEvent(WebViewEventType.NavigationError, new NavigationErrorDelegate(Element, errorResponse.StatusCode));
        }

        public override void OnPageStarted(Android.Webkit.WebView view, string url, Bitmap favicon)
        {
            if (((NavigationRequestedDelegate)Element.InvokeEvent(WebViewEventType.NavigationRequested, new NavigationRequestedDelegate(Element, url))).Cancel)
                view.StopLoading();
            else
                Element.SetValue(FormsWebView.SourceProperty, url);
        }

        public override void OnPageFinished(Android.Webkit.WebView view, string url)
        {
            Element.InvokeEvent(WebViewEventType.NavigationComplete, new NavigationCompletedDelegate(Element, url, true));
            Renderer.InjectJS(WebViewControlDelegate.InjectedFunction);

            foreach (var key in Element.GetGlobalCallbacks())
                Renderer.InjectJS(WebViewControlDelegate.GenerateFunctionScript(key));

            foreach (var key in Element.GetLocalCallbacks())
                Renderer.InjectJS(WebViewControlDelegate.GenerateFunctionScript(key));

            Element.InvokeEvent(WebViewEventType.ContentLoaded, new ContentLoadedDelegate(Element, url));
            base.OnPageFinished(view, url);
        }
    }
}