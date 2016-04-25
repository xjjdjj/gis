#region << 版 本 注 释 >>
/*
 * ========================================================================
 * Copyright(c)  陈锋, All Rights Reserved.
 * ========================================================================
 * CLR 版本：       4.0.30319.261
 * 类 名 称：       clsRangeQuery
 * 机器名称：       GIS-FLYH
 * 命名空间：       AYKJ.GISInterface
 * 文 件 名：       clsRangeQuery
 * 创建时间：       2012/7/16 15:09:11
 * 作    者：       陈锋
 * 功能说明：       专题查询
 * 修改时间：
 * 修 改 人：
 * ========================================================================
*/
#endregion

using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Geometry;

namespace AYKJ.GISStatistics
{
    /// <summary>
    /// 委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void RangeQueryDelegate(object sender, EventArgs e);

    public class clsRangeQuery
    {
        //所有的专业点数据
        private List<Graphic> lstGra;
        //定义Geometry服务
        private GeometryService geometryservice;
        //返回符合条件的数据
        public List<Graphic> lstReturnGraphic;
        //定义查询事件
        public event RangeQueryDelegate RangeQueryEvent;
        public event RangeQueryDelegate RangeQueryFaildEvent;
        //生成的Buff区域
        public Graphic BuffGraphic;
        //Buff半径
        private int intDistance;
        private SpatialReference oldSpatialReference;

        void ProcessAction(object sender, EventArgs e)
        {
            if (RangeQueryEvent == null)
                RangeQueryEvent += new RangeQueryDelegate(RangeQueryErrorEvent);
            RangeQueryEvent(sender, e);
        }

        /// <summary>
        /// 如果没有自己指定关联方法，将会调用该方法抛出错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RangeQueryErrorEvent(object sender, EventArgs e)
        {
            RangeQueryFaildEvent(sender, e);
        }

        /// <summary>
        /// Buff区域查询
        /// </summary>
        /// <param name="strUrl">Geometry服务地址</param>
        /// <param name="geotmp">查询的空间区域</param>
        /// <param name="intRadius">Buff半径</param>
        /// <param name="lsttmp">被查询的专题数据集合</param>
        public void RangeBuffQuery(string strUrl, ESRI.ArcGIS.Client.Geometry.Geometry geotmp, int intRadius, List<Graphic> lsttmp)
        {
            oldSpatialReference = geotmp.SpatialReference;
            Graphic gratmp = new Graphic();
            gratmp.Geometry = geotmp;
            intDistance = intRadius;
            lstReturnGraphic = new List<Graphic>();
            lstGra = lsttmp;
            geometryservice = new GeometryService(strUrl);

            #region 将现有坐标系统转换到21480坐标系
            geometryservice.Failed -= geometryservice_Failed;
            geometryservice.Failed += new EventHandler<TaskFailedEventArgs>(geometryservice_Failed);
            geometryservice.ProjectCompleted -= geometryservice_ProjectCompleted;
            geometryservice.ProjectCompleted += new EventHandler<GraphicsEventArgs>(geometryservice_ProjectCompleted);
            List<Graphic> lstprogra = new List<Graphic>();
            lstprogra.Add(gratmp);
            geometryservice.ProjectAsync(lstprogra, new ESRI.ArcGIS.Client.Geometry.SpatialReference(21480), "First");
            #endregion
        }

        void geometryservice_Failed(object sender, TaskFailedEventArgs e)
        {
            RangeQueryFaildEvent(sender, e);
        }

        /// <summary>
        /// 区域查询
        /// </summary>
        /// <param name="strUrl">Geometry服务地址</param>
        /// <param name="geotmp">查询的空间区域</param>
        /// <param name="lsttmp">被查询的专题数据集合</param>
        public void RangeQuery(string strUrl, ESRI.ArcGIS.Client.Geometry.Geometry geotmp, List<Graphic> lsttmp)
        {
            lstReturnGraphic = new List<Graphic>();
            lstGra = lsttmp;
            geometryservice = new GeometryService(strUrl);
            geometryservice.Failed -= geometryservice_Failed;
            geometryservice.Failed += new EventHandler<TaskFailedEventArgs>(geometryservice_Failed);
            geometryservice.RelationCompleted -= geometryservice_RelationCompleted;
            geometryservice.RelationCompleted += new EventHandler<RelationEventArgs>(geometryservice_RelationCompleted);

            Graphic gratmp = new Graphic();
            gratmp.Geometry = geotmp;
            List<Graphic> lst1 = new List<Graphic>();
            lst1.Add(gratmp);
            if (geometryservice.IsBusy == false)
            {
                geometryservice.RelationAsync(lst1, lstGra, GeometryRelation.esriGeometryRelationIntersection, null, lst1);
            }
        }

        /// <summary>
        /// 坐标转换结束
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometryservice_ProjectCompleted(object sender, GraphicsEventArgs e)
        {
            //转换成21480坐标系后进行Buff操作
            if (e.UserState.ToString() == "First")
            {
                Graphic gratmp = new Graphic();
                gratmp.Geometry = e.Results[0].Geometry;
                geometryservice.BufferCompleted -= geometryservice_BufferCompleted;
                geometryservice.BufferCompleted += new EventHandler<GraphicsEventArgs>(geometryservice_BufferCompleted);
                BufferParameters bufferParams = new BufferParameters()
                {
                    BufferSpatialReference = gratmp.Geometry.SpatialReference,
                    OutSpatialReference = gratmp.Geometry.SpatialReference,
                    Unit = LinearUnit.Meter,
                };
                bufferParams.Distances.Add(intDistance);
                bufferParams.Features.Add(gratmp);
                geometryservice.BufferAsync(bufferParams);
            }
            //转换后的原有坐标系统进行拓扑分析
            else if (e.UserState.ToString() == "Second")
            {
                geometryservice.RelationCompleted -= geometryservice_RelationCompleted;
                geometryservice.RelationCompleted += new EventHandler<RelationEventArgs>(geometryservice_RelationCompleted);
                BuffGraphic = e.Results[0];
                List<Graphic> lst1 = new List<Graphic>();
                lst1.Add(BuffGraphic);
                if (geometryservice.IsBusy == false)
                {
                    geometryservice.RelationAsync(lst1, lstGra, GeometryRelation.esriGeometryRelationIntersection, null, lst1);
                }
            }
        }

        /// <summary>
        /// 生成区域路径,并将21480坐标系转换成原有坐标系统
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometryservice_BufferCompleted(object sender, GraphicsEventArgs e)
        {
            geometryservice.ProjectCompleted -= geometryservice_ProjectCompleted;
            geometryservice.ProjectCompleted += new EventHandler<GraphicsEventArgs>(geometryservice_ProjectCompleted);
            geometryservice.ProjectAsync(e.Results, oldSpatialReference, "Second");
        }

        /// <summary>
        /// 返回Graphic数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometryservice_RelationCompleted(object sender, RelationEventArgs e)
        {
            if (e.Results.Count != 0)
            {
                for (int i = 0; i < e.Results.Count; i++)
                {
                    lstReturnGraphic.Add(lstGra[e.Results[i].Graphic2Index]);
                }
            }
            ProcessAction(this, EventArgs.Empty);
        }
    }
}

