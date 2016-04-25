/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 16:01:09  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// WebServiceTools说明：
/// 唯一标识：06f04aad-27f4-4e51-8224-b19c3290ff92  
/// </summary>

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace AYKJ.GISDevelop.Platform.ToolKit
{
    public class WebServiceTools
    {
        public static dynamic BasicWebService(Type type, string url, int maxBufferSize = 2147483647,
        int maxReceivedMessageSize = 2147483647, long closeTimeOut = 600, long openTimeout = 600,
            long receiveTimeout = 600, long sendTimeout = 600)
        {
            BasicHttpBinding basicBinding = new BasicHttpBinding();
            basicBinding.MaxBufferSize = maxBufferSize;
            basicBinding.MaxReceivedMessageSize = maxReceivedMessageSize;
            CustomBinding binding = new CustomBinding(basicBinding);
            binding.CloseTimeout = TimeSpan.FromSeconds(closeTimeOut);
            binding.OpenTimeout = TimeSpan.FromSeconds(openTimeout);
            binding.ReceiveTimeout = TimeSpan.FromSeconds(receiveTimeout);
            binding.SendTimeout = TimeSpan.FromSeconds(sendTimeout);


            EndpointAddress endPoint = new EndpointAddress(new Uri(url, UriKind.RelativeOrAbsolute));
            //创建对象
            dynamic service = Activator.CreateInstance(type, binding, endPoint);
            return service;
        }
    }
}
