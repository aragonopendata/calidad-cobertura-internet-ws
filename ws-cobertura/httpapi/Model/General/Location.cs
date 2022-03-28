using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ws_cobertura.httpapi.Model.Response;

namespace ws_cobertura.httpapi.Model.General
{
    public class Location
    {
        protected static String EPSG25830_ETRS89 = "PROJCS[\"ETRS89 / UTM zone 30N\",GEOGCS[\"ETRS89\",DATUM[\"European_Terrestrial_Reference_System_1989\",SPHEROID[\"GRS 1980\",6378137,298.257222101,AUTHORITY[\"EPSG\",\"7019\"]],AUTHORITY[\"EPSG\",\"6258\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4258\"]],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"latitude_of_origin\",0],PARAMETER[\"central_meridian\",-3],PARAMETER[\"scale_factor\",0.9996],PARAMETER[\"false_easting\",500000],PARAMETER[\"false_northing\",0],AUTHORITY[\"EPSG\",\"25830\"],AXIS[\"Easting\",EAST],AXIS[\"Northing\",NORTH]]";
        protected static String EPSG4326_WGS84 = "GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]]";
        protected static String EPSG3857_WGS84 = "PROJCS[\"WGS 84 / Pseudo-Mercator\",GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]],PROJECTION[\"Mercator_1SP\"],PARAMETER[\"central_meridian\",0],PARAMETER[\"scale_factor\",1],PARAMETER[\"false_easting\",0],PARAMETER[\"false_northing\",0],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],AXIS[\"X\",EAST],AXIS[\"Y\",NORTH],EXTENSION[\"PROJ4\",\"+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +wktext  +no_defs\"],AUTHORITY[\"EPSG\",\"3857\"]]";
        //r EPSG:25830.
        public double utm_X { get; set; } // utm conversion latitud
        public double utm_Y { get; set; } // utm conversion longitud
        public double geo_x { get; set; } // latitud
        public double geo_y { get; set; } // longitud

        public void Reproject(double latitude, double longitude)
        {
            GeoAPI.CoordinateSystems.ICoordinateSystem cs_FROM = ProjNet.Converters.WellKnownText.CoordinateSystemWktReader.Parse(EPSG4326_WGS84, Encoding.UTF8) as GeoAPI.CoordinateSystems.ICoordinateSystem;
            GeoAPI.CoordinateSystems.ICoordinateSystem cs_TO = ProjNet.Converters.WellKnownText.CoordinateSystemWktReader.Parse(EPSG25830_ETRS89, Encoding.UTF8) as GeoAPI.CoordinateSystems.ICoordinateSystem;

            ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory ctfac = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
            GeoAPI.CoordinateSystems.Transformations.ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(cs_FROM, cs_TO);

            double[] fromPoint = new double[] { longitude, latitude };
            double[] toPoint = trans.MathTransform.Transform(fromPoint);

            this.geo_x = latitude;
            this.geo_y = longitude;

            this.utm_X = toPoint[0];
            this.utm_Y = toPoint[1];
        }

        public void InverseReproject(double X, double Y)
        {
            GeoAPI.CoordinateSystems.ICoordinateSystem cs_FROM = ProjNet.Converters.WellKnownText.CoordinateSystemWktReader.Parse(EPSG25830_ETRS89, Encoding.UTF8) as GeoAPI.CoordinateSystems.ICoordinateSystem;
            GeoAPI.CoordinateSystems.ICoordinateSystem cs_TO = ProjNet.Converters.WellKnownText.CoordinateSystemWktReader.Parse(EPSG4326_WGS84, Encoding.UTF8) as GeoAPI.CoordinateSystems.ICoordinateSystem;

            ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory ctfac = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
            GeoAPI.CoordinateSystems.Transformations.ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(cs_FROM, cs_TO);

            double[] fromPoint = new double[] { X, Y };
            double[] toPoint = trans.MathTransform.Transform(fromPoint);

            this.geo_x = toPoint[1];
            this.geo_y = toPoint[0];

            this.utm_X = X; // geo_x;
            this.utm_Y = Y; // geo_y;
        }

        public bool IsGeodataValid()
        {
            return this.utm_X != 0 && this.utm_Y != 0;
        }
    }
}
