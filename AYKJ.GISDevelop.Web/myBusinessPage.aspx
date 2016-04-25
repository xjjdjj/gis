<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="myBusinessPage.aspx.cs"
    Inherits="AYKJ.GISDevelop.Web.myBusinessPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        p.titleStyleClass
        {
            margin: 0px;
            padding: 5px;
            text-align: center;
            background: #3c3c3c;
            border: solid 1px;
            font-weight: bold;
            color: #ffffff;
            height: 20px;
            cursor: move;
        }
        
        div.linkBtnStyleClass
        {
            margin: 0px;
            padding: 5px;
            background: #3c3c3c;
            border: solid 1px;
            color: #9ff806;
            display: none;
        }
    </style>
</head>
<body style="width: 100%; height: 100%; margin: 0px; overflow: hidden;">
    <form id="form1" runat="server">
    <asp:HiddenField runat="server" ID="xxxx" Value="" />
    <p id="pTitle" class="titleStyleClass">
        GIS平台模拟业务页面</p>
    <div id="divInput" class="linkBtnStyleClass">
        <table>
            <tr>
                <td style="font-size: 12px;">
                    编号id:
                    <input type="text" id="d1" style="width: 60px;" value="2d4b9f4b82374fb4bd02cacc47346929"/>
                </td>
                <td style="font-size: 12px;">
                    类型type:
                    <input type="text" id="d2" style="width: 60px;" value="zdwxy" />
                </td>
                <td style="font-size: 12px;">
                    单位代码dwdm:
                    <input type="text" id="d3" style="width: 60px;" value="11111111-1" />
                </td>
                <td style="font-size: 12px;">
                    名称remark:
                    <input type="text" id="d4" style="width: 60px;" value="化工厂" />
                </td>
                <td style="font-size: 12px;">
                    经度x:
                    <input type="text" id="d5" style="width: 60px;" />
                </td>
                <td style="font-size: 12px;">
                    纬度y:
                    <input type="text" id="d6" style="width: 60px;" />
                </td>
                <td style="font-size: 12px;">
                    街道:
                    <input type="text" id="d7" style="width: 60px;" />
                </td>
                <td style="font-size: 12px;">
                    企业类型:
                    <input type="text" id="d8" style="width: 60px;" />
                </td>
            </tr>
        </table>
        <table>
            <tr>
                <td align="left">
                    <input type="button" value="江宁登录" onclick="selectClick(this)" style="background-color: White;" />
                    <input type="button" value="删除数据" onclick="selectClick(this)" style="background-color: Gray;" />
                    <input type="button" value="定位" onclick="selectClick(this)" style="background-color: Gray;" />
                    <!--<input type="button" value="打点" onclick="selectClick(this)" style="background-color: Gray;" />-->
                    <input type="button" value="GPS打点" onclick="selectClick(this)" style="background-color: White;" />
                    <input type="button" value="无坐标打点" onclick="selectClick(this)" style="background-color: Gray;" />
                    <input type="button" value="爆炸模拟" onclick="selectClick(this)" style="background-color: White;" />
                    <input type="button" value="火灾模拟" onclick="selectClick(this)" style="background-color: White;" />
                    <input type="button" value="瞬时泄漏模拟" onclick="selectClick(this)" style="background-color: Gray;" />
                    <input type="button" value="连续泄漏模拟" onclick="selectClick(this)" style="background-color: Gray;" />
                    <input type="button" value="最短逃生路径" onclick="selectClick(this)" style="background-color: White;" />
                    <input type="button" value="最短救援路径" onclick="selectClick(this)" style="background-color: White;" />
                    <input type="button" value="爆炸模拟(受体分析)" onclick="selectClick(this)" style="background-color: Gray;" />
                    <input type="button" value="火灾模拟(受体分析)" onclick="selectClick(this)" style="background-color: Gray;" />
                    <input type="button" value="瞬时泄漏模拟(受体分析)" onclick="selectClick(this)" style="background-color: White;" />
                    <input type="button" value="连续泄漏模拟(受体分析)" onclick="selectClick(this)" style="background-color: White;" />
                    <input type="button" value="自动搜索" onclick="selectClick(this)" style="background-color: Gray;" />
                    <input type="button" value="自动搜索(全)" onclick="selectClick(this)" style="background-color: Gray;" />
                    <input type="button" value="饼状图统计" onclick="selectClick(this)" style="background-color: White;" />
                    <input type="button" value="柱状图统计" onclick="selectClick(this)" style="background-color: White;" />
                    <input type="button" value="GPS定位" onclick="selectClick(this)" style="background-color: Gray;" />
                    <input type="button" value="停止定位" onclick="selectClick(this)" style="background-color: Gray;" />
                    <input type="button" value="轨迹回放" onclick="selectClick(this)" style="background-color: Gray;" />
                    <input type="button" value="停止回放" onclick="selectClick(this)" style="background-color: Gray;" />
                    <input type="button" value="爆炸模拟(疏散路径)" onclick="selectClick(this)" style="background-color: White;" />
                    <input type="button" value="火灾模拟(疏散路径)" onclick="selectClick(this)" style="background-color: White;" />
                    <input type="button" value="瞬时泄漏模拟(疏散路径)" onclick="selectClick(this)" style="background-color: Gray;" />
                    <input type="button" value="连续泄漏模拟(疏散路径)" onclick="selectClick(this)" style="background-color: Gray;" />
                    <input type="button" value="专题数据输出" onclick="selectClick(this)" style="background-color: White;" />
                    <input type="button" value="半径查询" onclick="selectClick(this)" style="background-color: White;" />
                    <input type="button" value="半径查询(不画圆)" onclick="selectClick(this)" style="background-color: White;" />
                    <input type="button" value="半径查询(画圆)" onclick="selectClick(this)" style="background-color: White;" />
                    <input type="button" value="电子标绘" onclick="selectClick(this)" style="background-color: White;" />
                </td>
            </tr>
        </table>
    </div>
    <iframe frameborder="0" marginwidth="0" marginheight="0" scrolling="no" id="mainHtml"
        height="700" width="100%" src="AYKJ.GISDevelopTestPage.aspx"></iframe>
    <%--            <iframe frameborder="0" marginwidth="0" marginheight="0" scrolling="no" id="mainHtml"
        src="http://10.0.0.119/AYKJ/Develop/AYKJ.GISDevelopTestPage.aspx" name="parentFrame"></iframe>--%>
    </form>
</body>
<script type="text/javascript" src="ClientBin/ICONS/jQuery.js"></script>
<script type="text/javascript">
    //窗口适应（IE）(适用于模拟业务系统上下结构的布局。)++++++++++++++++++++++++++++++++++++++++++
    $(document).ready(function () {
        $(".titleStyleClass").click(function () {
            $(".linkBtnStyleClass").slideToggle("slow", headResize);
        });

    });

    function headResize() {
        document.getElementById("mainHtml").style.width = document.documentElement.clientWidth;

        var hoh = $(".linkBtnStyleClass").height();
        var toh = $(".titleStyleClass").height();

        var mainWin = document.documentElement.clientHeight - toh;

        if (mainWin > 300) {

            if ($(".linkBtnStyleClass").is(':hidden')) {
                document.getElementById("mainHtml").style.height = mainWin - 16;
            }
            else {
                document.getElementById("mainHtml").style.height = mainWin - hoh - 29;
            }
        }
    }


    window.onload = function () {
        document.getElementById("mainHtml").style.width = document.documentElement.clientWidth;

        var hoh = $(".linkBtnStyleClass").height();
        var toh = $(".titleStyleClass").height();

        var mainWin = document.documentElement.clientHeight - toh;

        if (mainWin > 300) {

            if ($(".linkBtnStyleClass").is(':hidden')) {
                document.getElementById("mainHtml").style.height = mainWin - 16;
            }
            else {
                document.getElementById("mainHtml").style.height = mainWin - hoh - 29;
            }
        }

        window.onresize = headResize;
    }

    //类结构
    function clsBusinessWxy(dwdm, wxyid, wxytype, remark, x, y) {
        var sp = new Object;
        sp.dwdm = dwdm;
        sp.wxyid = wxyid;
        sp.wxytype = wxytype;
        sp.remark = remark;
        sp.x = x;
        sp.y = y;

        return sp;
    }


    //江宁类结构
    function clsBusinessWxy_Jn(dwdm, wxyid, wxytype, remark, x, y, street, enttype) {
        var sp = new Object;
        sp.dwdm = dwdm;
        sp.wxyid = wxyid;
        sp.wxytype = wxytype;
        sp.remark = remark;
        sp.x = x;
        sp.y = y;
        sp.street = street;
        sp.enttype = enttype;

        return sp;
    }

    //调用功能类型.本函数作为中间函数，主要用于函数参数的生成。
    function selectClick(v) {
        var action = v.value;
        //alert(action);
        switch (action) {
            case "江宁登录": //第二个参数“role”是角色权限名：街道名/安监局
                var role = document.getElementById("d8").value;
                if (role == "") {
                    alert("登录角色不可为空");
                    return;
                }
                innerClick(action, role, null, null, null, null, null);
                break;
            case "电子标绘":
                innerClick(action, null, null, null, null, null, null);
                break;
            case "半径查询(不画圆)":
                var wxyid = document.getElementById("d1").value;
                if (wxyid == "") {
                    alert("wxyid不可为空");
                    return;
                }
                var wxytype = document.getElementById("d2").value;
                if (wxytype == "") {
                    alert("wxytype不可为空");
                    return;
                }
                var dwdm = document.getElementById("d3").value;
                if (dwdm == "") {
                    alert("dwdm不可为空");
                    return;
                }
                var remark = document.getElementById("d4").value;
                if (remark == "") {
                    alert("remark不可为空");
                    return;
                }

                var clsWxy = clsBusinessWxy();
                clsWxy.dwdm = dwdm;
                clsWxy.wxyid = wxyid;
                clsWxy.wxytype = wxytype;
                clsWxy.remark = remark;
                clsWxy.x = "";
                clsWxy.y = "";

                var querytype = ["enterprise"];
                var radius = "1000";
                innerClick(action, radius, querytype, clsWxy, null, null, null);
                break;
            case "半径查询(画圆)":
                var wxyid = document.getElementById("d1").value;
                if (wxyid == "") {
                    alert("wxyid不可为空");
                    return;
                }
                var wxytype = document.getElementById("d2").value;
                if (wxytype == "") {
                    alert("wxytype不可为空");
                    return;
                }
                var dwdm = document.getElementById("d3").value;
                if (dwdm == "") {
                    alert("dwdm不可为空");
                    return;
                }
                var remark = document.getElementById("d4").value;
                if (remark == "") {
                    alert("remark不可为空");
                    return;
                }

                var clsWxy = clsBusinessWxy();
                clsWxy.dwdm = dwdm;
                clsWxy.wxyid = wxyid;
                clsWxy.wxytype = wxytype;
                clsWxy.remark = remark;
                clsWxy.x = "";
                clsWxy.y = "";

                var querytype = ["enterprise"];
                var radius = "500";
                innerClick(action, radius, querytype, clsWxy, null, null, null);
                break;
            case "半径查询":
                var wxytype = document.getElementById("d2").value;
                var clsWxy = clsBusinessWxy();
                clsWxy.wxytype = wxytype;
                clsWxy.wxyid = "radius";
                innerClick(action, null, null, clsWxy, null, null, null);
                break;
            case "删除数据":
                var wxyid = document.getElementById("d1").value;
                if (wxyid == "") {
                    alert("wxyid不可为空");
                    return;
                }
                var wxytype = document.getElementById("d2").value;
                if (wxytype == "") {
                    alert("wxytype不可为空");
                    return;
                }
                var dwdm = document.getElementById("d3").value;
                if (dwdm == "") {
                    alert("dwdm不可为空");
                    return;
                }
                var remark = document.getElementById("d4").value;
                if (remark == "") {
                    alert("remark不可为空");
                    return;
                }

                var clsWxy = clsBusinessWxy();
                clsWxy.dwdm = dwdm;
                clsWxy.wxyid = wxyid;
                clsWxy.wxytype = wxytype;
                clsWxy.remark = remark;
                clsWxy.x = "";
                clsWxy.y = "";

                innerClick(action, null, null, clsWxy, null, null, null);
                break;
            case "打点":
                var wxyid = document.getElementById("d1").value;
                if (wxyid == "") {
                    alert("wxyid不可为空");
                    return;
                }
                var wxytype = document.getElementById("d2").value;
                if (wxytype == "") {
                    alert("wxytype不可为空");
                    return;
                }
                var dwdm = document.getElementById("d3").value;
                if (dwdm == "") {
                    alert("dwdm不可为空");
                    return;
                }
                var remark = document.getElementById("d4").value;
                if (remark == "") {
                    alert("remark不可为空");
                    return;
                }
                var x = document.getElementById("d5").value;
                if (x == "") {
                    alert("x不可为空");
                    return;
                }
                var y = document.getElementById("d6").value;
                if (y == "") {
                    alert("y不可为空");
                    return;
                }

                var clsWxy = clsBusinessWxy();
                clsWxy.dwdm = dwdm;
                clsWxy.wxyid = wxyid;
                clsWxy.wxytype = wxytype;
                clsWxy.remark = remark;
                clsWxy.x = x;
                clsWxy.y = y;

                innerClick(action, null, null, clsWxy, null, null, null);
                break;
            case "GPS打点":
                var wxyid = document.getElementById("d1").value;
                if (wxyid == "") {
                    alert("wxyid不可为空");
                    return;
                }
                var wxytype = document.getElementById("d2").value;
                if (wxytype == "") {
                    alert("wxytype不可为空");
                    return;
                }
                var dwdm = document.getElementById("d3").value;
                if (dwdm == "") {
                    alert("dwdm不可为空");
                    return;
                }
                var remark = document.getElementById("d4").value;
                if (remark == "") {
                    alert("remark不可为空");
                    return;
                }
                var x = document.getElementById("d5").value;
                if (x == "") {
                    alert("x不可为空");
                    return;
                }
                var y = document.getElementById("d6").value;
                if (y == "") {
                    alert("y不可为空");
                    return;
                }

                //20150305：江宁智慧安监
                var street = document.getElementById("d7").value;
                var enttype = document.getElementById("d8").value;

                var clsWxy = clsBusinessWxy_Jn();
                clsWxy.dwdm = dwdm;
                clsWxy.wxyid = wxyid;
                clsWxy.wxytype = wxytype;
                clsWxy.remark = remark;
                clsWxy.x = x;
                clsWxy.y = y;
                clsWxy.street = street;
                clsWxy.enttype = enttype;

                innerClick(action, null, null, clsWxy, null, null, null);
                break;
            case "无坐标打点":
                var wxyid = document.getElementById("d1").value;
                if (wxyid == "") {
                    alert("wxyid不可为空");
                    return;
                }
                var wxytype = document.getElementById("d2").value;
                if (wxytype == "") {
                    alert("wxytype不可为空");
                    return;
                }
                var dwdm = document.getElementById("d3").value;
                if (dwdm == "") {
                    alert("dwdm不可为空");
                    return;
                }
                var remark = document.getElementById("d4").value;
                if (remark == "") {
                    alert("remark不可为空");
                    return;
                }
                //20150305：江宁智慧安监
                var street = document.getElementById("d7").value;
                var enttype = document.getElementById("d8").value;

                var clsWxy = clsBusinessWxy_Jn();
                clsWxy.dwdm = dwdm;
                clsWxy.wxyid = wxyid;
                clsWxy.wxytype = wxytype;
                clsWxy.remark = remark;
                clsWxy.x = "";
                clsWxy.y = "";
                clsWxy.street = street;
                clsWxy.enttype = enttype;

                innerClick(action, null, null, clsWxy, null, null, null);
                break;
            case "定位":
                var wxyid = document.getElementById("d1").value;
                if (wxyid == "") {
                    alert("wxyid不可为空");
                    return;
                }
                var wxytype = document.getElementById("d2").value;
                if (wxytype == "") {
                    alert("wxytype不可为空");
                    return;
                }
                var dwdm = document.getElementById("d3").value;
                if (dwdm == "") {
                    alert("dwdm不可为空");
                    return;
                }
                var remark = document.getElementById("d4").value;
                if (remark == "") {
                    alert("remark不可为空");
                    return;
                }

                var clsWxy = clsBusinessWxy();
                clsWxy.dwdm = dwdm;
                clsWxy.wxyid = wxyid;
                clsWxy.wxytype = wxytype;
                clsWxy.remark = remark;
                clsWxy.x = "";
                clsWxy.y = "";

                innerClick(action, null, null, clsWxy, null, null, null);
                break;
            case "爆炸模拟":
                var wxyid = document.getElementById("d1").value;
                if (wxyid == "") {
                    alert("wxyid不可为空");
                    return;
                }
                var wxytype = document.getElementById("d2").value;
                if (wxytype == "") {
                    alert("wxytype不可为空");
                    return;
                }
                var dwdm = document.getElementById("d3").value;
                if (dwdm == "") {
                    alert("dwdm不可为空");
                    return;
                }
                var remark="{\"wxyid\":\""+wxyid+"\",\"wxytype\":\""+wxytype+"\",\"dwdm\":\""+dwdm+"\"}";

                //影响范围
                var sDeath = 200;
                var sSeriouslyInjured = 500;
                var sMinorInjuries = 700;
                var sSafe = 800;

                var aryStr = new Array();
                aryStr[0] = sDeath.toString();
                aryStr[1] = sSeriouslyInjured.toString();
                aryStr[2] = sMinorInjuries.toString();
                aryStr[3] = sSafe.toString();

                innerClick(action, remark, aryStr, null, null, null, null);
                break;
            case "火灾模拟":
                var wxyid = document.getElementById("d1").value;
                if (wxyid == "") {
                    alert("wxyid不可为空");
                    return;
                }
                var wxytype = document.getElementById("d2").value;
                if (wxytype == "") {
                    alert("wxytype不可为空");
                    return;
                }
                var dwdm = document.getElementById("d3").value;
                if (dwdm == "") {
                    alert("dwdm不可为空");
                    return;
                }
                var remark = "{\"wxyid\":\"" + wxyid + "\",\"wxytype\":\"" + wxytype + "\",\"dwdm\":\"" + dwdm + "\"}";

                //影响范围
                var sDeath =80;
                var sSeriouslyInjured = 130;
                var sMinorInjuries = 346;
                var sSafe = 459;

                var aryStr = new Array();
                aryStr[0] = sDeath.toString();
                aryStr[1] = sSeriouslyInjured.toString();
                aryStr[2] = sMinorInjuries.toString();
                aryStr[3] = sSafe.toString();

                innerClick(action, remark, aryStr, null, null, null, null);
                break;
            case "瞬时泄漏模拟":

                var arysize = new Array();
                arysize[0] = 160;
                arysize[1] = 200;
                arysize[2] = 240;
                arysize[3] = 200;
                arysize[4] = 280;
                var aryradius = new Array();
                aryradius[0] = 50;
                aryradius[1] = 80;
                aryradius[2] = 100;
                aryradius[3] = 120;
                aryradius[4] = 80;
                aryradius[5] = 40;

                var wxyid = document.getElementById("d1").value;
                if (wxyid == "") {
                    alert("wxyid不可为空");
                    return;
                }
                var wxytype = document.getElementById("d2").value;
                if (wxytype == "") {
                    alert("wxytype不可为空");
                    return;
                }
                var dwdm = document.getElementById("d3").value;
                if (dwdm == "") {
                    alert("dwdm不可为空");
                    return;
                }
                var remark = "{\"wxyid\":\"" + wxyid + "\",\"wxytype\":\"" + wxytype + "\",\"dwdm\":\"" + dwdm + "\"}";

                //将array型参数打包
                var aryArr = new Array();
                aryArr[0] = arysize;
                aryArr[1] = aryradius;

                innerClick(action, remark, null, null, null, aryArr, null);
                break;
            case "连续泄漏模拟":

                //燃烧区点位数组
                var aryPoisoning = new Array();
                aryPoisoning[0] = 0.0;
                aryPoisoning[1] = 1.9299999999999318;
                aryPoisoning[2] = 3.6199999999998957;
                aryPoisoning[3] = 4.979999999999867;
                aryPoisoning[4] = 6.159999999999842;
                aryPoisoning[5] = 7.20999999999982;
                aryPoisoning[6] = 8.1699999999998;
                aryPoisoning[7] = 9.05999999999978;
                aryPoisoning[8] = 9.889999999999763;
                aryPoisoning[9] = 10.669999999999746;
                aryPoisoning[10] = 11.40999999999973;
                aryPoisoning[11] = 12.109999999999715;
                aryPoisoning[12] = 12.779999999999724;
                aryPoisoning[13] = 13.419999999999824;
                aryPoisoning[14] = 14.02999999999992;
                aryPoisoning[15] = 14.620000000000012;
                aryPoisoning[16] = 15.1800000000001;
                aryPoisoning[17] = 15.730000000000185;
                aryPoisoning[18] = 16.250000000000266;
                aryPoisoning[19] = 16.750000000000345;
                aryPoisoning[20] = 17.24000000000042;
                aryPoisoning[21] = 17.710000000000495;
                aryPoisoning[22] = 18.160000000000565;
                aryPoisoning[23] = 18.590000000000632;
                aryPoisoning[24] = 19.010000000000698;
                aryPoisoning[25] = 19.41000000000076;
                aryPoisoning[26] = 19.80000000000082;
                aryPoisoning[27] = 20.18000000000088;
                aryPoisoning[28] = 20.540000000000937;
                aryPoisoning[29] = 20.89000000000099;
                aryPoisoning[30] = 21.220000000001043;
                aryPoisoning[31] = 21.540000000001093;
                aryPoisoning[32] = 21.850000000001142;
                aryPoisoning[33] = 22.140000000001187;
                aryPoisoning[34] = 22.42000000000123;
                aryPoisoning[35] = 22.690000000001273;
                aryPoisoning[36] = 22.950000000001314;
                aryPoisoning[37] = 23.200000000001353;
                aryPoisoning[38] = 23.43000000000139;
                aryPoisoning[39] = 23.650000000001423;
                aryPoisoning[40] = 23.860000000001456;
                aryPoisoning[41] = 24.060000000001487;
                aryPoisoning[42] = 24.240000000001515;
                aryPoisoning[43] = 24.420000000001544;
                aryPoisoning[44] = 24.58000000000157;
                aryPoisoning[45] = 24.730000000001592;
                aryPoisoning[46] = 24.860000000001612;
                aryPoisoning[47] = 24.990000000001633;
                aryPoisoning[48] = 25.10000000000165;
                aryPoisoning[49] = 25.210000000001667;
                aryPoisoning[50] = 25.30000000000168;
                aryPoisoning[51] = 25.370000000001692;
                aryPoisoning[52] = 25.440000000001703;
                aryPoisoning[53] = 25.49000000000171;
                aryPoisoning[54] = 25.530000000001717;
                aryPoisoning[55] = 25.560000000001722;
                aryPoisoning[56] = 25.580000000001725;
                aryPoisoning[57] = 25.580000000001725;
                aryPoisoning[58] = 25.570000000001723;
                aryPoisoning[59] = 25.55000000000172;
                aryPoisoning[60] = 25.510000000001714;
                aryPoisoning[61] = 25.460000000001706;
                aryPoisoning[62] = 25.400000000001697;
                aryPoisoning[63] = 25.320000000001684;
                aryPoisoning[64] = 25.23000000000167;
                aryPoisoning[65] = 25.120000000001653;
                aryPoisoning[66] = 25.000000000001634;
                aryPoisoning[67] = 24.860000000001612;
                aryPoisoning[68] = 24.71000000000159;
                aryPoisoning[69] = 24.540000000001562;
                aryPoisoning[70] = 24.360000000001534;
                aryPoisoning[71] = 24.160000000001503;
                aryPoisoning[72] = 23.94000000000147;
                aryPoisoning[73] = 23.70000000000143;
                aryPoisoning[74] = 23.450000000001392;
                aryPoisoning[75] = 23.170000000001348;
                aryPoisoning[76] = 22.880000000001303;
                aryPoisoning[77] = 22.560000000001253;
                aryPoisoning[78] = 22.2300000000012;
                aryPoisoning[79] = 21.870000000001145;
                aryPoisoning[80] = 21.480000000001084;
                aryPoisoning[81] = 21.07000000000102;
                aryPoisoning[82] = 20.640000000000953;
                aryPoisoning[83] = 20.17000000000088;
                aryPoisoning[84] = 19.680000000000803;
                aryPoisoning[85] = 19.16000000000072;
                aryPoisoning[86] = 18.590000000000632;
                aryPoisoning[87] = 18.00000000000054;
                aryPoisoning[88] = 17.36000000000044;
                aryPoisoning[89] = 16.670000000000332;
                aryPoisoning[90] = 15.940000000000218;
                aryPoisoning[91] = 15.150000000000095;
                aryPoisoning[92] = 14.299999999999962;
                aryPoisoning[93] = 13.369999999999816;
                aryPoisoning[94] = 12.34999999999971;
                aryPoisoning[95] = 11.239999999999734;
                aryPoisoning[96] = 9.97999999999976;
                aryPoisoning[97] = 8.55999999999979;
                aryPoisoning[98] = 6.8899999999998265;
                aryPoisoning[99] = 4.819999999999871;
                aryPoisoning[100] = 0;

                //中毒区点位数组
                var aryCombustion = new Array();
                for (var i = 0; i < aryPoisoning.length; i++) {
                    aryCombustion[i] = aryPoisoning[i] + 10;
                }
                                aryCombustion[100] = 0;
                

                //泄漏点信息
                var wxyid = document.getElementById("d1").value;
                if (wxyid == "") {
                    alert("wxyid不可为空");
                    return;
                }
                var wxytype = document.getElementById("d2").value;
                if (wxytype == "") {
                    alert("wxytype不可为空");
                    return;
                }
                var dwdm = document.getElementById("d3").value;
                if (dwdm == "") {
                    alert("dwdm不可为空");
                    return;
                }
                var remark = "{\"wxyid\":\"" + wxyid + "\",\"wxytype\":\"" + wxytype + "\",\"dwdm\":\"" + dwdm + "\"}";
                

                //燃烧区步长
                var dbCombustionStep = 1.03;

                //中毒区步长
                var dbPoisoningStep = 2.45;

                //燃烧偏移量
                var dbCombustionStart = 0;

                //燃烧区面积
                var CombustionArea = 1860;

                //中毒偏移量
                var dbPoisoningStart = 0;

                //中毒区面积
                var PoisoningArea = 356;

                //为0则展示燃烧和中毒，1为燃烧，2为中毒
                var strtype = 0;

                var aryStr = new Array();
                aryStr[0] = dbCombustionStep.toString();
                aryStr[1] = dbPoisoningStep.toString();
                aryStr[2] = dbCombustionStart.toString();
                aryStr[3] = CombustionArea.toString();
                aryStr[4] = dbPoisoningStart.toString();
                aryStr[5] = PoisoningArea.toString();0
                aryStr[6] = strtype.toString();


                //将array型参数打包
                var aryArr = new Array();
                aryArr[0] = aryCombustion;
                aryArr[1] = aryPoisoning;

                innerClick(action, remark, null, null, aryStr, aryArr, null);
                break;
            case "最短逃生路径":
                var wxyid = document.getElementById("d1").value;
                var wxytype = document.getElementById("d2").value;
                var dwdm = document.getElementById("d3").value;
                if (wxyid == "" && wxytype == "" && dwdm == "")
                {
                    var remark = "{\"wxyid\":\"" + "765295373" + "\",\"wxytype\":\"" + "enterprise" + "\",\"dwdm\":\"" + "765295373" + "\"}";
                }
                else
                {
                    var remark = "{\"wxyid\":\"" + wxyid + "\",\"wxytype\":\"" + wxytype + "\",\"dwdm\":\"" + dwdm + "\"}";
                }
                //多个终点信息
                var aryStr = new Array();
                aryStr[0] = "{\"wxyid\":\"0a26f99ea9ed49be82f338ebed86ce6f\",\"wxytype\":\"mdeical\",\"dwdm\":\"明基医院\"}";
                aryStr[1] = "{\"wxyid\":\"3e508ba209b14ee3a97b6347749262f4\",\"wxytype\":\"shelter\",\"dwdm\":\"大行宫应急避难\"}";
                innerClick(action, remark, aryStr, null, null, null, null);
                break;
            case "最短救援路径":

                //多个起点信息
                var aryStr = new Array();
                aryStr[0] = "{\"wxyid\":\"78068339\",\"wxytype\":\"enterprise\",\"dwdm\":\"78068339\"}";
                aryStr[1] = "{\"wxyid\":\"73316170-9\",\"wxytype\":\"enterprise\",\"dwdm\":\"73316170-9\"}";
                
                //终点信息
                var wxyid = document.getElementById("d1").value;
                var wxytype = document.getElementById("d2").value;
                var dwdm = document.getElementById("d3").value;
                if (wxyid == "" && wxytype == "" && dwdm == "") {
                    var remark = "{\"wxyid\":\"" + "320114000030913" + "\",\"wxytype\":\"" + "enterprise" + "\",\"dwdm\":\"" + "320114000030913" + "\"}";
                }
                else {
                    var remark = "{\"wxyid\":\"" + wxyid + "\",\"wxytype\":\"" + wxytype + "\",\"dwdm\":\"" + dwdm + "\"}";
                }

                innerClick(action, remark, aryStr, null, null, null, null);
                break;
            case "爆炸模拟(受体分析)":
                //影响范围
                var sDeath = 100;
                var sSeriouslyInjured = 387;
                var sMinorInjuries =450;
                var sSafe = 600;

                var aryStr = new Array();
                aryStr[0] = sDeath.toString();
                aryStr[1] = sSeriouslyInjured.toString();
                aryStr[2] = sMinorInjuries.toString();
                aryStr[3] = sSafe.toString();

                var wxyid = document.getElementById("d1").value;
                var wxytype = document.getElementById("d2").value;
                var dwdm = document.getElementById("d3").value;
                var remark="{\"wxyid\":\""+wxyid+"\",\"wxytype\":\""+wxytype+"\",\"dwdm\":\""+dwdm+"\"}";

                if (wxyid == "" && wxytype == "" && dwdm == "") {
                    innerClick(action, null, aryStr, null, null, null, null);
                }
                else {
                    innerClick(action, remark, aryStr, null, null, null, null);
                }
                break;
            case "火灾模拟(受体分析)":
                //影响范围
                var sDeath =100;
                var sSeriouslyInjured = 500;
                var sMinorInjuries = 700;
                var sSafe = 800;

                var aryStr = new Array();
                aryStr[0] = sDeath.toString();
                aryStr[1] = sSeriouslyInjured.toString();
                aryStr[2] = sMinorInjuries.toString();
                aryStr[3] = sSafe.toString();

                var wxyid = document.getElementById("d1").value;
                var wxytype = document.getElementById("d2").value;
                var dwdm = document.getElementById("d3").value;
                var remark = "{\"wxyid\":\"" + wxyid + "\",\"wxytype\":\"" + wxytype + "\",\"dwdm\":\"" + dwdm + "\"}";
                if (wxyid == "" && wxytype == "" && dwdm == "") {
                    innerClick(action, null, aryStr, null, null, null, null);
                }
                else {
                    innerClick(action, remark, aryStr, null, null, null, null);
                }
                break;
            case "瞬时泄漏模拟(受体分析)":
                var arysize = [160, 200];

                //                arysize[1] = 160;
                //                arysize[1] = 200;
                //                arysize[2] = 240;
                //                arysize[3] = 200;
                //                arysize[4] = 280;
                var aryradius = [6.484240252400426, 57.1696478724288];
                //                aryradius[1] = 51;
                //                aryradius[1] = 80;
                //                aryradius[2] = 100;
                //                aryradius[3] = 120;
                //                aryradius[4] = 80;
                //                aryradius[5] = 40;

                //将array型参数打包
                var aryArr = new Array();
                aryArr[0] = arysize;
                aryArr[1] = aryradius;
                var ary = new Array();
                //                    ary[0] = 80;
                var wxyid = document.getElementById("d1").value;
                var wxytype = document.getElementById("d2").value;
                var dwdm = document.getElementById("d3").value;
                var remark = "{\"wxyid\":\"" + wxyid + "\",\"wxytype\":\"" + wxytype + "\",\"dwdm\":\"" + dwdm + "\"}";

                if (wxyid == "" && wxytype == "" && dwdm == "") {
                    innerClick(action, null, ary, null, null, aryArr, null);
                }
                else {
                    innerClick(action, remark, ary, null, null, aryArr, null);
                }
                break;
            case "连续泄漏模拟(受体分析)":
                //燃烧区点位数组
                var aryPoisoning = new Array();
                aryPoisoning[0] = 0.0;
                aryPoisoning[1] = 1.8499999999999002;
                aryPoisoning[2] = 4.3699999999998465;
                aryPoisoning[3] = 6.389999999999803;
                aryPoisoning[4] = 8.139999999999766;
                aryPoisoning[5] = 9.709999999999733;
                aryPoisoning[6] = 11.15999999999971;
                aryPoisoning[7] = 12.49999999999992;
                aryPoisoning[8] = 13.770000000000119;
                aryPoisoning[9] = 14.960000000000305;
                aryPoisoning[10] = 16.100000000000485;
                aryPoisoning[11] = 17.19000000000065;
                aryPoisoning[12] = 18.230000000000814;
                aryPoisoning[13] = 19.23000000000097;
                aryPoisoning[14] = 20.19000000000112;
                aryPoisoning[15] = 21.110000000001264;
                aryPoisoning[16] = 22.000000000001407;
                aryPoisoning[17] = 22.86000000000154;
                aryPoisoning[18] = 23.690000000001668;
                aryPoisoning[19] = 24.490000000001793;
                aryPoisoning[20] = 25.260000000001916;
                aryPoisoning[21] = 26.01000000000203;
                aryPoisoning[22] = 26.740000000002148;
                aryPoisoning[23] = 27.43000000000214;
                aryPoisoning[24] = 28.110000000002003;
                aryPoisoning[25] = 28.760000000001874;
                aryPoisoning[26] = 29.39000000000175;
                aryPoisoning[27] = 30.000000000001627;
                aryPoisoning[28] = 30.59000000000151;
                aryPoisoning[29] = 31.1500000000014;
                aryPoisoning[30] = 31.70000000000129;
                aryPoisoning[31] = 32.23000000000118;
                aryPoisoning[32] = 32.730000000001084;
                aryPoisoning[33] = 33.22000000000099;
                aryPoisoning[34] = 33.680000000000895;
                aryPoisoning[35] = 34.130000000000805;
                aryPoisoning[36] = 34.56000000000072;
                aryPoisoning[37] = 34.96000000000064;
                aryPoisoning[38] = 35.35000000000056;
                aryPoisoning[39] = 35.72000000000049;
                aryPoisoning[40] = 36.07000000000042;
                aryPoisoning[41] = 36.41000000000035;
                aryPoisoning[42] = 36.72000000000029;
                aryPoisoning[43] = 37.02000000000023;
                aryPoisoning[44] = 37.29000000000018;
                aryPoisoning[45] = 37.550000000000125;
                aryPoisoning[46] = 37.79000000000008;
                aryPoisoning[47] = 38.01000000000003;
                aryPoisoning[48] = 38.209999999999994;
                aryPoisoning[49] = 38.38999999999996;
                aryPoisoning[50] = 38.549999999999926;
                aryPoisoning[51] = 38.699999999999896;
                aryPoisoning[52] = 38.81999999999987;
                aryPoisoning[53] = 38.92999999999985;
                aryPoisoning[54] = 39.009999999999835;
                aryPoisoning[55] = 39.07999999999982;
                aryPoisoning[56] = 39.11999999999981;
                aryPoisoning[57] = 39.14999999999981;
                aryPoisoning[58] = 39.14999999999981;
                aryPoisoning[59] = 39.13999999999981;
                aryPoisoning[60] = 39.09999999999982;
                aryPoisoning[61] = 39.03999999999983;
                aryPoisoning[62] = 38.959999999999845;
                aryPoisoning[63] = 38.849999999999866;
                aryPoisoning[64] = 38.72999999999989;
                aryPoisoning[65] = 38.57999999999992;
                aryPoisoning[66] = 38.409999999999954;
                aryPoisoning[67] = 38.209999999999994;
                aryPoisoning[68] = 37.99000000000004;
                aryPoisoning[69] = 37.74000000000009;
                aryPoisoning[70] = 37.47000000000014;
                aryPoisoning[71] = 37.1700000000002;
                aryPoisoning[72] = 36.840000000000266;
                aryPoisoning[73] = 36.48000000000034;
                aryPoisoning[74] = 36.10000000000041;
                aryPoisoning[75] = 35.6800000000005;
                aryPoisoning[76] = 35.23000000000059;
                aryPoisoning[77] = 34.75000000000068;
                aryPoisoning[78] = 34.240000000000784;
                aryPoisoning[79] = 33.680000000000895;
                aryPoisoning[80] = 33.09000000000101;
                aryPoisoning[81] = 32.46000000000114;
                aryPoisoning[82] = 31.79000000000127;
                aryPoisoning[83] = 31.070000000001414;
                aryPoisoning[84] = 30.300000000001567;
                aryPoisoning[85] = 29.48000000000173;
                aryPoisoning[86] = 28.610000000001904;
                aryPoisoning[87] = 27.67000000000209;
                aryPoisoning[88] = 26.670000000002133;
                aryPoisoning[89] = 25.60000000000197;
                aryPoisoning[90] = 24.44000000000179;
                aryPoisoning[91] = 23.200000000001594;
                aryPoisoning[92] = 21.85000000000138;
                aryPoisoning[93] = 20.37000000000115;
                aryPoisoning[94] = 18.7600000000009;
                aryPoisoning[95] = 16.96000000000062;
                aryPoisoning[96] = 14.940000000000301;
                aryPoisoning[97] = 12.619999999999939;
                aryPoisoning[98] = 9.84999999999973;
                aryPoisoning[99] = 6.299999999999805;
                aryPoisoning[100] = 0.0;

                //中毒区点位数组
                var aryCombustion = new Array();
//                for (var i = 0; i < aryPoisoning.length; i++) {
//                    aryCombustion[i] = aryPoisoning[i] + 10;
//                }
//                aryCombustion[100] = 0;
                aryCombustion[0] = 0.0;
                aryCombustion[1] = 1.8499999999999002;
                aryCombustion[2] = 4.3699999999998465;
                aryCombustion[3] = 6.389999999999803;
                aryCombustion[4] = 8.139999999999766;
                aryCombustion[5] = 9.709999999999733;
                aryCombustion[6] = 11.15999999999971;
                aryCombustion[7] = 12.49999999999992;
                aryCombustion[8] = 13.770000000000119;
                aryCombustion[9] = 14.960000000000305;
                aryCombustion[10] = 16.100000000000485;
                aryCombustion[11] = 17.19000000000065;
                aryCombustion[12] = 18.230000000000814;
                aryCombustion[13] = 19.23000000000097;
                aryCombustion[14] = 20.19000000000112;
                aryCombustion[15] = 21.110000000001264;
                aryCombustion[16] = 22.000000000001407;
                aryCombustion[17] = 22.86000000000154;
                aryCombustion[18] = 23.690000000001668;
                aryCombustion[19] = 24.490000000001793;
                aryCombustion[20] = 25.260000000001916;
                aryCombustion[21] = 26.01000000000203;
                aryCombustion[22] = 26.740000000002148;
                aryCombustion[23] = 27.43000000000214;
                aryCombustion[24] = 28.110000000002003;
                aryCombustion[25] = 28.760000000001874;
                aryCombustion[26] = 29.39000000000175;
                aryCombustion[27] = 30.000000000001627;
                aryCombustion[28] = 30.59000000000151;
                aryCombustion[29] = 31.1500000000014;
                aryCombustion[30] = 31.70000000000129;
                aryCombustion[31] = 32.23000000000118;
                aryCombustion[32] = 32.730000000001084;
                aryCombustion[33] = 33.22000000000099;
                aryCombustion[34] = 33.680000000000895;
                aryCombustion[35] = 34.130000000000805;
                aryCombustion[36] = 34.56000000000072;
                aryCombustion[37] = 34.96000000000064;
                aryCombustion[38] = 35.35000000000056;
                aryCombustion[39] = 35.72000000000049;
                aryCombustion[40] = 36.07000000000042;
                aryCombustion[41] = 36.41000000000035;
                aryCombustion[42] = 36.72000000000029;
                aryCombustion[43] = 37.02000000000023;
                aryCombustion[44] = 37.29000000000018;
                aryCombustion[45] = 37.550000000000125;
                aryCombustion[46] = 37.79000000000008;
                aryCombustion[47] = 38.01000000000003;
                aryCombustion[48] = 38.209999999999994;
                aryCombustion[49] = 38.38999999999996;
                aryCombustion[50] = 38.549999999999926;
                aryCombustion[51] = 38.699999999999896;
                aryCombustion[52] = 38.81999999999987;
                aryCombustion[53] = 38.92999999999985;
                aryCombustion[54] = 39.009999999999835;
                aryCombustion[55] = 39.07999999999982;
                aryCombustion[56] = 39.11999999999981;
                aryCombustion[57] = 39.14999999999981;
                aryCombustion[58] = 39.14999999999981;
                aryCombustion[59] = 39.13999999999981;
                aryCombustion[60] = 39.09999999999982;
                aryCombustion[61] = 39.03999999999983;
                aryCombustion[62] = 38.959999999999845;
                aryCombustion[63] = 38.849999999999866;
                aryCombustion[64] = 38.72999999999989;
                aryCombustion[65] = 38.57999999999992;
                aryCombustion[66] = 38.409999999999954;
                aryCombustion[67] = 38.209999999999994;
                aryCombustion[68] = 37.99000000000004;
                aryCombustion[69] = 37.74000000000009;
                aryCombustion[70] = 37.47000000000014;
                aryCombustion[71] = 37.1700000000002;
                aryCombustion[72] = 36.840000000000266;
                aryCombustion[73] = 36.48000000000034;
                aryCombustion[74] = 36.10000000000041;
                aryCombustion[75] = 35.6800000000005;
                aryCombustion[76] = 35.23000000000059;
                aryCombustion[77] = 34.75000000000068;
                aryCombustion[78] = 34.240000000000784;
                aryCombustion[79] = 33.680000000000895;
                aryCombustion[80] = 33.09000000000101;
                aryCombustion[81] = 32.46000000000114;
                aryCombustion[82] = 31.79000000000127;
                aryCombustion[83] = 31.070000000001414;
                aryCombustion[84] = 30.300000000001567;
                aryCombustion[85] = 29.48000000000173;
                aryCombustion[86] = 28.610000000001904;
                aryCombustion[87] = 27.67000000000209;
                aryCombustion[88] = 26.670000000002133;
                aryCombustion[89] = 25.60000000000197;
                aryCombustion[90] = 24.44000000000179;
                aryCombustion[91] = 23.200000000001594;
                aryCombustion[92] = 21.85000000000138;
                aryCombustion[93] = 20.37000000000115;
                aryCombustion[94] = 18.7600000000009;
                aryCombustion[95] = 16.96000000000062;
                aryCombustion[96] = 14.940000000000301;
                aryCombustion[97] = 12.619999999999939;
                aryCombustion[98] = 9.84999999999973;
                aryCombustion[99] = 6.299999999999805;
                aryCombustion[100] = 0.0;

                //燃烧区步长
                var dbCombustionStep = 1.03;
                //中毒区步长
                var dbPoisoningStep = 2.45;
                //燃烧偏移量
                var dbCombustionStart = 0;
                //燃烧区面积
                var CombustionArea = 0;
                //中毒偏移量
                var dbPoisoningStart = 0;
                //中毒区面积
                var PoisoningArea = 0;
                //为0则展示燃烧和中毒，1为燃烧，2为中毒
                var strtype = 0;

                var aryStr = new Array();
                aryStr[0] = dbCombustionStep.toString();
                aryStr[1] = dbPoisoningStep.toString();
                aryStr[2] = dbCombustionStart.toString();
                aryStr[3] = CombustionArea.toString();
                aryStr[4] = dbPoisoningStart.toString();
                aryStr[5] = PoisoningArea.toString();
                aryStr[6] = strtype.toString();
                //风向
                //                    aryStr[7] = "123";

                //将array型参数打包
                var aryArr = new Array();
                aryArr[0] =  aryPoisoning;
                aryArr[1] = aryCombustion;


                //泄漏点信息
                var wxyid = document.getElementById("d1").value;
                var wxytype = document.getElementById("d2").value;
                var dwdm = document.getElementById("d3").value;
                var remark = "{\"wxyid\":\"" + wxyid + "\",\"wxytype\":\"" + wxytype + "\",\"dwdm\":\"" + dwdm + "\"}";
               // var remark = "{\"wxyid\":\"" + "003654931" + "\",\"wxytype\":\"" + "WXY_A" + "\",\"dwdm\":\"" + "003654931" + "\"}";
//                if (wxyid == "" && wxytype == "" && dwdm=="") {
//                    innerClick(action, null, null, null, aryStr, aryArr, null);
//                }
//                else {
                    innerClick(action, remark, null, null, aryStr, aryArr, null);
//                }
                break;
            case "自动搜索":

                var wxyid = document.getElementById("d1").value;
                var wxytype = document.getElementById("d2").value;
                var dwdm = document.getElementById("d3").value;
                
                if (wxyid == "" && wxytype == "" && dwdm == "") {
                    innerClick(action, null, null, null, null, null, null);
                }
                else {
                    var remark = "{\"wxyid\":\"" + wxyid + "\",\"wxytype\":\"" + wxytype + "\",\"dwdm\":\"" + dwdm + "\",\"every\":\"" + 2000 + "\",\"all\":\"" + 12000 + "\"}";
                    innerClick(action, remark, null, null, null, null, null);
                }
                break;
            case "自动搜索(全)":
                var wxyid = document.getElementById("d1").value;
                var wxytype = document.getElementById("d2").value;
                var dwdm = document.getElementById("d3").value;

                if (wxyid == "" && wxytype == "" && dwdm == "") {
                    innerClick(action, null, null, null, null, null, null);
                }
                else {
                    var remark = "{\"wxyid\":\"" + wxyid + "\",\"wxytype\":\"" + wxytype + "\",\"dwdm\":\"" + dwdm + "\",\"every\":\"" + 2000 + "\",\"all\":\"" + 12000 + "\"}";
                    innerClick(action, remark, null, null, null, null, null);
                }
                break;
            case "柱状图统计":
                var strlayer = "物质仓库|应急机构|救援队伍|救援物资|救援装备|通信保障|运输保障|医疗救护|技术支持|避难场所|企业|危险场所|煤矿|非煤矿山|尾矿库";
                innerClick(action, strlayer, null, null, null, null, null);
                break;
            case "饼状图统计":
                var strlayer = "物质仓库|应急机构|救援队伍|救援物资|救援装备|通信保障|运输保障|医疗救护|技术支持|避难场所|企业|危险场所|煤矿|非煤矿山|尾矿库";
                innerClick(action, strlayer, null, null, null, null, null);
                break;
            case "GPS定位":
//                var strids = "ty_1|ty_2";
                //                var strtypes = "巡逻车|巡逻车";

                var strids = "fzjj002";
                var strtypes = "巡逻车";
                var aryStr = new Array();
                aryStr[0] = strids;
                aryStr[1] = strtypes;
                innerClick(action, null, null, null, aryStr, null, null);
                break;
            case "停止定位":
                innerClick(action, null, null, null, null, null, null);
                break;
            case "轨迹回放":
                var strinfo = "fzjj002|20120914154137991|20120914154700753";
                var strtype = "巡逻车";
                var aryStr = new Array();
                aryStr[0] = strinfo;
                aryStr[1] = strtype;
                innerClick(action, null, null, null, aryStr, null, null);
                break;
            case "停止回放":
                innerClick(action, null, null, null, null, null, null);
                break;
            case "爆炸模拟(疏散路径)":
                //影响范围
                var sDeath = 120;
                var sSeriouslyInjured = 320;
                var sMinorInjuries = 480;
                var sSafe = 640;

                var aryStr = new Array();
                aryStr[0] = sDeath.toString();
                aryStr[1] = sSeriouslyInjured.toString();
                aryStr[2] = sMinorInjuries.toString();
                aryStr[3] = sSafe.toString();

                var ary1 = new Array();
                ary1[0] = "{\"wxyid\":\"4\",\"wxytype\":\"responseteam\",\"dwdm\":\"4\"}";
                var ary2 = new Array();
                ary2[0] = "{\"wxyid\":\"7\",\"wxytype\":\"shelter\",\"dwdm\":\"7\"}";
                var aryArr = new Array();
                aryArr[0] = ary1;
                aryArr[1] = ary2;

                var wxyid = document.getElementById("d1").value;
                var wxytype = document.getElementById("d2").value;
                var dwdm = document.getElementById("d3").value;
                var remark = "{\"wxyid\":\"" + wxyid + "\",\"wxytype\":\"" + wxytype + "\",\"dwdm\":\"" + dwdm + "\"}";
                if (wxyid == "" && wxytype == "" && dwdm == "") {
                    innerClick(action, null, aryStr, null, null, aryArr, null);
                }
                else {
                    innerClick(action, remark, aryStr, null, null, aryArr, null);
                }
                break;
            case "火灾模拟(疏散路径)":
                //影响范围
                var sDeath = 200;
                var sSeriouslyInjured = 440;
                var sMinorInjuries = 630;
                var sSafe = 800;

                var aryStr = new Array();
                aryStr[0] = sDeath.toString();
                aryStr[1] = sSeriouslyInjured.toString();
                aryStr[2] = sMinorInjuries.toString();
                aryStr[3] = sSafe.toString();

                var ary1 = new Array();
                ary1[0] = "{\"wxyid\":\"4\",\"wxytype\":\"responseteam\",\"dwdm\":\"4\"}";
                var ary2 = new Array();
                ary2[0] = "{\"wxyid\":\"7\",\"wxytype\":\"shelter\",\"dwdm\":\"7\"}";
                var aryArr = new Array();
                aryArr[0] = ary1;
                aryArr[1] = ary2;

                var wxyid = document.getElementById("d1").value;
                var wxytype = document.getElementById("d2").value;
                var dwdm = document.getElementById("d3").value;
                var remark = "{\"wxyid\":\"" + wxyid + "\",\"wxytype\":\"" + wxytype + "\",\"dwdm\":\"" + dwdm + "\"}";
                if (wxyid == "" && wxytype == "" && dwdm == "") {
                    innerClick(action, null, aryStr, null, null, aryArr, null);
                }
                else {
                    innerClick(action, remark, aryStr, null, null, aryArr, null);
                }
                break;
            case "瞬时泄漏模拟(疏散路径)":
                var arysize = [6.484240252400426, 57.1696478724288, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
//                arysize[0] = 160;
//                arysize[1] = 200;
//                arysize[2] = 240;
//                arysize[3] = 200;
//                arysize[4] = 280;
                var aryradius = [18, 82, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
//                aryradius[0] = 50;
//                aryradius[1] = 80;
//                aryradius[2] = 100;
//                aryradius[3] = 120;
//                aryradius[4] = 80;
//                aryradius[5] = 40;

                //将array型参数打包
                var aryArr = new Array();
                aryArr[0] = arysize;
                aryArr[1] = aryradius;
                var ary = new Array();
                //ary[0] = 80;

                var ary1 = new Array();
                ary1[0] = "{\"wxyid\":\"4\",\"wxytype\":\"responseteam\",\"dwdm\":\"4\"}";
                var ary2 = new Array();
                ary2[0] = "{\"wxyid\":\"7\",\"wxytype\":\"shelter\",\"dwdm\":\"7\"}";
                aryArr[2] = ary1;
                aryArr[3] = ary2;

                var wxyid = document.getElementById("d1").value;
                var wxytype = document.getElementById("d2").value;
                var dwdm = document.getElementById("d3").value;
                var remark = "{\"wxyid\":\"" + wxyid + "\",\"wxytype\":\"" + wxytype + "\",\"dwdm\":\"" + dwdm + "\"}";
                if (wxyid == "" && wxytype == "" && dwdm == "") {
                    innerClick(action, null, ary, null, null, aryArr, null);
                }
                else {
                    innerClick(action, remark, ary, null, null, aryArr, null);
                }
                break;
            case "连续泄漏模拟(疏散路径)":
                //燃烧区点位数组
                var aryCombustion = new Array();
                aryCombustion[0] = 0.0;
                aryCombustion[1] = 1.9299999999999318;
                aryCombustion[2] = 3.6199999999998957;
                aryCombustion[3] = 4.979999999999867;
                aryCombustion[4] = 6.159999999999842;
                aryCombustion[5] = 7.20999999999982;
                aryCombustion[6] = 8.1699999999998;
                aryCombustion[7] = 9.05999999999978;
                aryCombustion[8] = 9.889999999999763;
                aryCombustion[9] = 10.669999999999746;
                aryCombustion[10] = 11.40999999999973;
                aryCombustion[11] = 12.109999999999715;
                aryCombustion[12] = 12.779999999999724;
                aryCombustion[13] = 13.419999999999824;
                aryCombustion[14] = 14.02999999999992;
                aryCombustion[15] = 14.620000000000012;
                aryCombustion[16] = 15.1800000000001;
                aryCombustion[17] = 15.730000000000185;
                aryCombustion[18] = 16.250000000000266;
                aryCombustion[19] = 16.750000000000345;
                aryCombustion[20] = 17.24000000000042;
                aryCombustion[21] = 17.710000000000495;
                aryCombustion[22] = 18.160000000000565;
                aryCombustion[23] = 18.590000000000632;
                aryCombustion[24] = 19.010000000000698;
                aryCombustion[25] = 19.41000000000076;
                aryCombustion[26] = 19.80000000000082;
                aryCombustion[27] = 20.18000000000088;
                aryCombustion[28] = 20.540000000000937;
                aryCombustion[29] = 20.89000000000099;
                aryCombustion[30] = 21.220000000001043;
                aryCombustion[31] = 21.540000000001093;
                aryCombustion[32] = 21.850000000001142;
                aryCombustion[33] = 22.140000000001187;
                aryCombustion[34] = 22.42000000000123;
                aryCombustion[35] = 22.690000000001273;
                aryCombustion[36] = 22.950000000001314;
                aryCombustion[37] = 23.200000000001353;
                aryCombustion[38] = 23.43000000000139;
                aryCombustion[39] = 23.650000000001423;
                aryCombustion[40] = 23.860000000001456;
                aryCombustion[41] = 24.060000000001487;
                aryCombustion[42] = 24.240000000001515;
                aryCombustion[43] = 24.420000000001544;
                aryCombustion[44] = 24.58000000000157;
                aryCombustion[45] = 24.730000000001592;
                aryCombustion[46] = 24.860000000001612;
                aryCombustion[47] = 24.990000000001633;
                aryCombustion[48] = 25.10000000000165;
                aryCombustion[49] = 25.210000000001667;
                aryCombustion[50] = 25.30000000000168;
                aryCombustion[51] = 25.370000000001692;
                aryCombustion[52] = 25.440000000001703;
                aryCombustion[53] = 25.49000000000171;
                aryCombustion[54] = 25.530000000001717;
                aryCombustion[55] = 25.560000000001722;
                aryCombustion[56] = 25.580000000001725;
                aryCombustion[57] = 25.580000000001725;
                aryCombustion[58] = 25.570000000001723;
                aryCombustion[59] = 25.55000000000172;
                aryCombustion[60] = 25.510000000001714;
                aryCombustion[61] = 25.460000000001706;
                aryCombustion[62] = 25.400000000001697;
                aryCombustion[63] = 25.320000000001684;
                aryCombustion[64] = 25.23000000000167;
                aryCombustion[65] = 25.120000000001653;
                aryCombustion[66] = 25.000000000001634;
                aryCombustion[67] = 24.860000000001612;
                aryCombustion[68] = 24.71000000000159;
                aryCombustion[69] = 24.540000000001562;
                aryCombustion[70] = 24.360000000001534;
                aryCombustion[71] = 24.160000000001503;
                aryCombustion[72] = 23.94000000000147;
                aryCombustion[73] = 23.70000000000143;
                aryCombustion[74] = 23.450000000001392;
                aryCombustion[75] = 23.170000000001348;
                aryCombustion[76] = 22.880000000001303;
                aryCombustion[77] = 22.560000000001253;
                aryCombustion[78] = 22.2300000000012;
                aryCombustion[79] = 21.870000000001145;
                aryCombustion[80] = 21.480000000001084;
                aryCombustion[81] = 21.07000000000102;
                aryCombustion[82] = 20.640000000000953;
                aryCombustion[83] = 20.17000000000088;
                aryCombustion[84] = 19.680000000000803;
                aryCombustion[85] = 19.16000000000072;
                aryCombustion[86] = 18.590000000000632;
                aryCombustion[87] = 18.00000000000054;
                aryCombustion[88] = 17.36000000000044;
                aryCombustion[89] = 16.670000000000332;
                aryCombustion[90] = 15.940000000000218;
                aryCombustion[91] = 15.150000000000095;
                aryCombustion[92] = 14.299999999999962;
                aryCombustion[93] = 13.369999999999816;
                aryCombustion[94] = 12.34999999999971;
                aryCombustion[95] = 11.239999999999734;
                aryCombustion[96] = 9.97999999999976;
                aryCombustion[97] = 8.55999999999979;
                aryCombustion[98] = 6.8899999999998265;
                aryCombustion[99] = 4.819999999999871;
                aryCombustion[100] = 0;

                //中毒区点位数组
                var aryPoisoning = new Array();
                for (var i = 0; i < aryCombustion.length; i++) {
                    aryPoisoning[i] = aryCombustion[i] + 10;
                }
                aryPoisoning[100] = 0;

                //燃烧区步长
                var dbCombustionStep = 4;
                //中毒区步长
                var dbPoisoningStep = 6;
                //燃烧偏移量
                var dbCombustionStart = 0;
                //燃烧区面积
                var CombustionArea = 1860356;
                //中毒偏移量
                var dbPoisoningStart = 0;
                //中毒区面积
                var PoisoningArea = 356;
                //为0则展示燃烧和中毒，1为燃烧，2为中毒
                var strtype = 0;

                var aryStr = new Array();
                aryStr[0] = dbCombustionStep.toString();
                aryStr[1] = dbPoisoningStep.toString();
                aryStr[2] = dbCombustionStart.toString();
                aryStr[3] = CombustionArea.toString();
                aryStr[4] = dbPoisoningStart.toString();
                aryStr[5] = PoisoningArea.toString();
                aryStr[6] = strtype.toString();
                //风向
                //                    aryStr[7] = "123";

                //将array型参数打包
                var aryArr = new Array();
                aryArr[0] = aryCombustion;
                aryArr[1] = aryPoisoning;

                var ary1 = new Array();
                ary1[0] = "{\"wxyid\":\"4\",\"wxytype\":\"responseteam\",\"dwdm\":\"4\"}";
                var ary2 = new Array();
                ary2[0] = "{\"wxyid\":\"7\",\"wxytype\":\"shelter\",\"dwdm\":\"7\"}";
                
                aryArr[2] = ary1;
                aryArr[3] = ary2;

                //泄漏点信息
                var wxyid = document.getElementById("d1").value;
                var wxytype = document.getElementById("d2").value;
                var dwdm = document.getElementById("d3").value;
                var remark = "{\"wxyid\":\"" + wxyid + "\",\"wxytype\":\"" + wxytype + "\",\"dwdm\":\"" + dwdm + "\"}";
                if (wxyid == "" && wxytype == "" && dwdm == "") {
                    innerClick(action, null, null, null, aryStr, aryArr, null);
                }
                else {
                    innerClick(action, remark, null, null, aryStr, aryArr, null);
                }
                break;
            case "专题数据输出":
                innerClick(action, null, null, null, null, null, null);
                break;
            default: alert("未选择任何接口功能，请检查代码1。");
        }


    }

    //"action":调用功能类型，"parms":需要传入的参数
    function innerClick(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls) {
        //alert(oAction);
        switch (oAction) {
            case "江宁登录":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "电子标绘":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "半径查询(画圆)":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "半径查询(不画圆)":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "半径查询":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "删除数据":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "打点":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "GPS打点":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "无坐标打点":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "定位":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "爆炸模拟":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "火灾模拟":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "瞬时泄漏模拟":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "连续泄漏模拟":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "最短逃生路径":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "最短救援路径":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "爆炸模拟(受体分析)":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "火灾模拟(受体分析)":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "瞬时泄漏模拟(受体分析)":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "连续泄漏模拟(受体分析)":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "自动搜索":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "自动搜索(全)":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "饼状图统计":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "柱状图统计":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "GPS定位":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "停止定位":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "轨迹回放":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "停止回放":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "爆炸模拟(疏散路径)":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "火灾模拟(疏散路径)":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "瞬时泄漏模拟(疏散路径)":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "连续泄漏模拟(疏散路径)":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            case "专题数据输出":
                self.frames['mainHtml'].linkFromBusinessPage(oAction, oStr, oArr, oCls, oArrStr, oArrArr, oArrCls);
                break;
            default: alert("未选择任何接口功能，请检查代码2。");
        }
    }

    function queryrtn(rtnstr) {
        alert(rtnstr);
    }
</script>
</html>
