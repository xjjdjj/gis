using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace AYKJ.GISDevelop.Platform
{
    public class ArcGISTiledLayerForTDT : TiledMapServiceLayer
    {
        private TileInfo _tileInfo;
        public string _baseURL;
        public string _serviceMode;
        public string _imageFormat;
        public string _layerId;
        public string _tileMatrixSetId;
        public string _initlevel = "0-17";
        public string _streve = "-180|-90|180|90";

        public override void Initialize()
        {
            double dbtmp = 0.7031249999891485;
            double[] arydb = new double[20];
            for (int i = 0; i < arydb.Length; i++)
            {
                arydb[i] = dbtmp;
                dbtmp = dbtmp / 2;
            }
            double dbxmin = double.Parse(_streve.Split('|')[0]);
            double dbymin = double.Parse(_streve.Split('|')[1]);
            double dbxmax = double.Parse(_streve.Split('|')[2]);
            double dbymax = double.Parse(_streve.Split('|')[3]);
            Envelope eve = new Envelope() { XMin = dbxmin, YMin = dbymin, XMax = dbxmax, YMax = dbymax, SpatialReference = new SpatialReference(4490) };
            this.FullExtent = eve;
            this.SpatialReference = new SpatialReference(4490);
            this.TileInfo = _tileInfo = new TileInfo()
            {
                Height = 256,
                Width = 256,
                Origin = new MapPoint(-180, 90)
                {
                    SpatialReference = new SpatialReference(4490)
                },
                Lods = new Lod[int.Parse(_initlevel.Split('-')[1]) - int.Parse(_initlevel.Split('-')[0]) + 1]
            };
            double resolution = arydb[int.Parse(_initlevel.Split('-')[0]) - 1];// 0.0013732910156250009;
            for (int i = 0; i < TileInfo.Lods.Length; i++)
            {
                TileInfo.Lods[i] = new Lod() { Resolution = resolution };
                resolution /= 2;
            }
            base.Initialize();
        }

        public override string GetTileUrl(int level, int row, int col)
        {
            level = level + int.Parse(_initlevel.Split('-')[0]);
            string urlRequest = _baseURL + "?service=wmts&request=GetTile&version=1.0.0" +
                "&layer=" + _layerId + "&style=default&format=" + _imageFormat + "&serviceMode=" + _serviceMode +
                "&TileMatrixSet=" + _tileMatrixSetId + "&TileMatrix=" + level + "&TileRow=" + row + "&TileCol=" + col;
            return urlRequest;
        }

    }
}
