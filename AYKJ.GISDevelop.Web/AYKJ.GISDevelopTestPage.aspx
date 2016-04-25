﻿<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>南京安元科技GIS平台</title>
    <style type="text/css">
    html, body {
	    height: 100%;
	    overflow: auto;
    }
    body {
	    padding: 0;
	    margin: 0;
    }
    #silverlightControlHost {
	    height: 100%;
	    text-align:center;
    }
    /**
     windowless = false   
     begin
     **/
    .inputcss
    {
        border-collapse: collapse;
        border: solid 0px Transparent; background-color: Transparent;
    }
    .divInputcss
    {

        border: solid 0px Transparent; background-color:Transparent;
         position: absolute; display: none;  z-index:1000;

    }
    /**
    windowless = false
    end
     **/
    </style>
    <script type="text/javascript">
        //类结构
        function clsAspxWxy(dwdm, wxyid, wxytype, remark, x, y) {
            var sp = new Object;
            sp.dwdm = dwdm;
            sp.wxyid = wxyid;
            sp.wxytype = wxytype;
            sp.remark = remark;
            sp.x = x;
            sp.y = y;
            return sp;
        }

        //响应外部业务的接口调用
        function linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls) {
            //对于Array类型的非空参数需要重新处理
            var new_oArr = new Array();
            if (oArr != null) {
                for (var m = 0; m < oArr.length; m++) {
                    new_oArr[m] = oArr[m];
                }
            }

            //对于string的Array重新处理
            var new_oArrStr = new Array();
            if (oArrStr != null) {
                for (var i = 0; i < oArrStr.length; i++) {
                    new_oArrStr[i] = oArrStr[i];
                }
            }

            //对于Array的Array重新处理
            var new_oArrArr = new Array();
            if (oArrArr != null) {
                var tmp_oArr;
                for (var n = 0; n < oArrArr.length; n++) {
                    tmp_oArr = oArrArr[n];
                    //子Array。
                    var i_arr = new Array();
                    if (tmp_oArr != null) {
                        for (var p = 0; p < tmp_oArr.length; p++) {
                            i_arr[p] = tmp_oArr[p];
                        }
                        new_oArrArr[n] = i_arr;
                    }
                }
            }

            //对于class的Array重新处理
            var new_oArrCls = new Array();
            if (oArrCls != null) {
                for (var q = 0; q < oArrCls.length; q++) {
                    new_oArrCls[q] = oArrCls[q];
                }
            }
            //此处循环验证是否存在此控件，防止调用出错。
            var tId = setInterval(function () {

                var control = document.getElementById("zcobj");

                if (control != null) {

                    control.Content.Page.linkFromAspxPage(oAction, oStr, new_oArr, oCls, new_oArrStr, new_oArrArr, new_oArrCls);
                    clearInterval(tId);
                }

            }, 500);
        }

        //定位结果返回业务系统方法
        function locPointFinished(rtnstr) {
            //alert(rtnstr);
        }

        //打点结果返回业务系统方法
        function addPointFinished(rtnstr) {
            //alert(rtnstr);
            //window.top.loadAccidentObject();
            if (window.parent.loadAccidentObject != undefined && typeof window.parent.loadAccidentObject == 'function') {
                window.parent.loadAccidentObject();
            }
        }
           
        //应急资源推荐结果返回业务系统方法
        function QueryFinished(rtnstr) {
            //parent.searchCallBack(rtnstr);
            window.parent.gisRes = rtnstr;
            window.parent.searchCallBack(rtnstr);
        }

        //打开详细信息页面
        //function openDetailInfoPage(rtnstr) {
        //    alert(rtnstr);
        //}

        //重大危险源面板视频按钮
        function rtnShiPinFromSL(result) {
            var str = eval('(' + result + ')');
            var t_id = encodeURI(encodeURI(str.wxyid));
            //window.open("http://192.168.0.104:8080/hd_safe/jsp/yx/VideoMaintenance.jsp?corpCode="+dwdm+"&statusType=1");
            window.open("http://192.168.0.104:8080/hd_safe/autoJump.action?menuId=525&url=jsp/seeVideo/showVideoTree.jsp?entId='" + t_id + "'");
        }

        //重大危险源面板模拟量按钮
        function rtnMoNiLiangFromSL(result) {
            var str = eval('(' + result + ')');
            var t_id = encodeURI(encodeURI(str.wxyid));
            //window.open("http://192.168.0.104:8080/hd_safe/jsp/yx/VideoMaintenance.jsp?corpCode="+dwdm+"&statusType=1");
            window.open("http://192.168.0.104:8080/hd_safe/autoJump.action?menuId=525&url=jsp/seeVideo/showVideoTree.jsp?entId='" + t_id + "'");
        }

        function InitMapFinished(result) {
            //alert(result);//"success"
        }

        //20120723zc:响应其他业务页面的调用并显示返回值(s1对应配置文件的"menu name",s2表示要传入的值)
        function linkGetFromBusinessPage(s1, s2) {
            alert("GetFromBusinessPage:" + "调用：" + s1 + "。传入值：" + s2);

            var control = document.getElementById("zcobj");
            var rs = control.Content.Page.LinkGetDataFromThisAspxPage(s1, s2);

            alert("GisPlatfromAspxPage_getReturn: " + rs);
        }

        //20120726zc:调用截图功能
        function fnCapture(imgName) {

            //alert(112);
            var obj_window = window.open('http://www.baidu.com', '_blank');
            obj_window.opener = window;
            obj_window.focus();
            //var xx = document.getElementById("tt");
            //xx.Capture(imgName);
            //alert('截图已保存到 ' + imgName.replace('.bmp', '.jpg'));
            //var control = document.getElementById("zcobj");
            //control.Content.Page.LinkSaveImg();
        }

        //20121217:点击气泡打开新页面
        function showNewPage(url) {
            alert(132);
        }

        function DownLoadShp(url) {
            var obj_window = window.open(url, '_blank');
            //            obj_window.opener = window;
            //            obj_window.focus();
        }

        function askHtmlSayHello() {
            window.frames[window.frames.length - 1].sayHello();
        }

        //20150330：获取session，江宁项目。
        function getSysSession(gr) {
            var mySession;

            //获取业务系统登录的角色信息：街道名（“街道1”）或者安监局（“ajj”）。
            //街道名对应plat.xml文件中的JnStreet节点。安监局用户就用英文字母“ajj”赋值。
            mySession = "ajj";
            //mySession = "街道1";
            mySession = "汤山街道";
            //mySession = window.parent.getAddress();

            var lbl_seession = document.getElementById("mysession");
            if (lbl_seession != null) {

                //alert("默认值：" + lbl_seession.value + " -- " + "Session[‘SYSROLE’]值: " + mySession)

                lbl_seession.value = mySession;
            }
        }

        //20150514zc
        function getPmtUrl(zdwxy) {
            //20150526zc:将该危险源的全部信息传出，传出的“zdwxy”的格式为：wxytype|wxyid|dwdm|remark（名称）|街道|企业类型。
            //alert(zdwxy);
            //alert(zdwxy.split('|')[2]);

            var datas = window.parent.getIdByEntpmt(zdwxy.split('|')[2]);

            var projectPath = window.parent.getProjectPath();
            //通过java业务系统获取平面图直接的页面访问地址url
            var new_oArr = new Array();
            if (datas == "" || datas == undefined || datas.length == 0 || datas.imgs == "" || datas.imgs == undefined || datas.imgs.length == 0) {

            } else {
                for (var i = 0; i < datas.imgs.length; i++) {
                    new_oArr[i] = projectPath + "/" + datas.imgs[i];
                    //console.log(new_oArr[i]);
                }
            }


            // new_oArr = ["http://www.jianbihua.cc/uploads/allimg/140216/2-1402161R046112.jpg", "http://img3.3lian.com/2013/c4/14/d/4.jpg", "http://www.jianbihua.org/jianbihua/image/zonghelei/jbh100627114265b8a045b1e7fe9f.jpg"];
            

            //此处循环验证是否存在此控件，防止调用出错。
            var tId = setInterval(function () {

                var control = document.getElementById("zcobj");

                if (control != null) {

                    control.Content.Page.linkFromAspxPage("打开重大危险源平面图", datas.qymc + "重大危险源", new_oArr, new Array(), new Array(), new Array(), new Array());
                    clearInterval(tId);
                }

            }, 500);
        }

        //江宁项目返回空间分析中量出的平面图范围，以便定位
        function getSpatialGeometry(s) {
            alert(s);
        }

    </script>

    <script type="text/javascript">
        window.onload = function () {
            var url = window.location.href;
            var newurl = url.split("#");
            if (newurl.length == 2) {
                window.location.href = newurl[0];
            }
        }
        function onSilverlightError(sender, args) {
            var appSource = "";
            if (sender != null && sender != 0) {
                appSource = sender.getHost().Source;
            }

            var errorType = args.ErrorType;
            var iErrorCode = args.ErrorCode;

            if (errorType == "ImageError" || errorType == "MediaError") {
                return;
            }

            var errMsg = "Silverlight 应用程序中未处理的错误 " + appSource + "\n";

            errMsg += "代码: " + iErrorCode + "    \n";
            errMsg += "类别: " + errorType + "       \n";
            errMsg += "消息: " + args.ErrorMessage + "     \n";

            if (errorType == "ParserError") {
                errMsg += "文件: " + args.xamlFile + "     \n";
                errMsg += "行: " + args.lineNumber + "     \n";
                errMsg += "位置: " + args.charPosition + "     \n";
            }
            else if (errorType == "RuntimeError") {
                if (args.lineNumber != 0) {
                    errMsg += "行: " + args.lineNumber + "     \n";
                    errMsg += "位置: " + args.charPosition + "     \n";
                }
                errMsg += "方法名称: " + args.methodName + "     \n";
            }

            alert(errMsg);
        }
    </script>
</head>
<body  style="overflow:hidden;">
    <form id="form1" runat="server" style="height:100%;">
    <div id="silverlightControlHost">
        <!--20150330:江宁项目保存主动获取的session到"mysession"中-->
        <input id="mysession" type="hidden" value="ajj" runat="server" style="width:0; height:0;"/>
        <object data="data:application/x-silverlight-2," type="application/x-silverlight-2" width="100%" height="100%" id="zcobj">
            
          <param name="enableHtmlAccess" value="true" />
		  <param name="source" value="ClientBin/AYKJ.GISDevelop.xap"/>
		  <param name="onError" value="onSilverlightError" />
		  <param name="background" value="white" />
		  <param name="minRuntimeVersion" value="5.0.61118.0" />
		  <param name="autoUpgrade" value="false" />
          <!-- false 可以输入中文-->
          <param name="windowless" value="true" />
		  <a href="http://www.microsoft.com/getsilverlight" style="text-decoration:none">
 			  <img src="http://go.microsoft.com/fwlink/?LinkId=161376" alt="获取 Microsoft Silverlight" style="border-style:none"/>
		  </a>
	    </object>        
        <iframe id="_sl_historyFrame" style="visibility:hidden;height:0px;width:0px;border:0px"></iframe>        
        <!--<object classid="clsid:AB15B9A5-1573-448A-870C-2F60916CECD7" codebase="TestCab.CAB" style="visibility:hidden;"  width="0px"  height="0px" id="tt" name="tt"/>-->
    </div>
    </form>
</body>
</html>