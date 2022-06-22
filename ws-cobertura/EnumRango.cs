using System;
using ws_cobertura.httpapi.Model.General;

namespace ws_cobertura
{
    public class EnumRango
    {
        public enum Intensidad : int
        {
            Desconocido = 0, // 0
            Nula = 1,   // 1
            Baja = 2,      // 2
            Media = 3,     // 3
            Alta = 4      // 4
        }
        public enum Latencia : int
        {
            Desconocido = 0, // 0
            MuyBaja = 1,   // 1
            Baja = 2,      // 2
            Media = 3,     // 3
            Alta = 4,      // 4
            MuyAlta = 5    // 5
        }

        public enum VelocidadBajada : int
        {
            Desconocido = 0, // 0
            Baja = 1,   // 1
            Media = 2,      // 2
            Alta = 3,     // 3
            MuyAlta = 4
        }

        public enum VelocidadSubida : int
        {
            Desconocido = 0, // 0
            Baja = 1,   // 1
            Media = 2,      // 2
            Alta = 3,     // 3
            MuyAlta = 4
        }

        public static int normalizarRangoIntensidad(int iRango)
        {

            if (iRango < 1)
            {
                iRango = 0;
            }
            else if (iRango < 2)
            {
                iRango = 1;
            }
            else if (iRango < 3)
            {
                iRango = 2;
            }
            else if (iRango < 4)
            {
                iRango = 3;
            }
            else if (iRango >= 4)
            {
                iRango = 4;
            }

            return iRango;
        }

        public static string getStringValue(Intensidad oIntensidad) {

            string sStringVal = string.Empty;

            switch (oIntensidad) {
                case Intensidad.Desconocido:
                    sStringVal = "Desconocido";
                    break;
                /*case Intensidad.SinSenyal:
                    sStringVal = "Sin Señal";
                    break;
                case Intensidad.MuyBaja:
                    sStringVal = "Muy Baja";
                    break;*/
                case Intensidad.Nula:
                    sStringVal = "Nula";
                    break;
                case Intensidad.Baja:
                    sStringVal = "Baja";
                    break;
                case Intensidad.Media:
                    sStringVal = "Media";
                    break;
                case Intensidad.Alta:
                    sStringVal = "Alta";
                    break;
                /*case Intensidad.MuyAlta:
                    sStringVal = "Muy Alta";
                    break;*/
            }

            return sStringVal;
        }

        public static string getStringValue(VelocidadBajada oVelocidadBajada)
        {

            string sStringVal = string.Empty;

            switch (oVelocidadBajada)
            {
                case VelocidadBajada.Desconocido:
                    sStringVal = "Desconocido";
                    break;
                /*case VelocidadBajada.SinSenyal:
                    sStringVal = "Sin Señal";
                    break;*/
                case VelocidadBajada.Baja:
                    sStringVal = "Baja";
                    break;
                case VelocidadBajada.Media:
                    sStringVal = "Media";
                    break;
                case VelocidadBajada.Alta:
                    sStringVal = "Alta";
                    break;
                case VelocidadBajada.MuyAlta:
                    sStringVal = "Muy Alta";
                    break;
                /*case VelocidadBajada.MuyAlta:
                    sStringVal = "Muy Alta";
                    break;*/
            }

            return sStringVal;
        }

        public static string getStringValue(VelocidadSubida oVelocidadSubida)
        {

            string sStringVal = string.Empty;

            switch (oVelocidadSubida)
            {
                case VelocidadSubida.Desconocido:
                    sStringVal = "Desconocido";
                    break;
               /* case VelocidadSubida.SinSenyal:
                    sStringVal = "Sin Señal";
                    break;*/
                case VelocidadSubida.Baja:
                    sStringVal = "Baja";
                    break;
                case VelocidadSubida.Media:
                    sStringVal = "Media";
                    break;
                case VelocidadSubida.Alta:
                    sStringVal = "Alta";
                    break;
                case VelocidadSubida.MuyAlta:
                    sStringVal = "Muy Alta";
                    break;
                /*case VelocidadSubida.MuyAlta:
                    sStringVal = "Muy Alta";
                    break;*/
            }

            return sStringVal;
        }

        public static string getStringValue(Latencia oLatencia)
        {

            string sStringVal = string.Empty;

            switch (oLatencia)
            {
                case Latencia.Desconocido:
                    sStringVal = "Desconocido";
                    break;
                /*case Latencia.SinSenyal:
                    sStringVal = "Sin Señal";
                    break;*/
                case Latencia.MuyBaja:
                    sStringVal = "Muy Baja";
                    break;
                case Latencia.Baja:
                    sStringVal = "Baja";
                    break;
                case Latencia.Media:
                    sStringVal = "Media";
                    break;
                case Latencia.Alta:
                    sStringVal = "Alta";
                    break;
                case Latencia.MuyAlta:
                    sStringVal = "Muy Alta";
                    break;
            }

            return sStringVal;
        }

        public static EnumRango.Intensidad calcularRangoIntensidad(decimal? dValor, string tipoRed)
        {
            EnumRango.Intensidad oEnumRango = EnumRango.Intensidad.Desconocido;

            try
            {
                if (dValor.HasValue)
                {
                    int iValor = (int)dValor;

                    // viene dada en dbm y negativo, pej. -95 dbm
                    iValor = Math.Abs(iValor);

                    switch (tipoRed) {

                        case "2G":
                            if (iValor < 75)
                            {
                                oEnumRango = EnumRango.Intensidad.Alta;
                            }
                            else if (iValor < 80)
                            {
                                oEnumRango = EnumRango.Intensidad.Media;
                            }
                            else if (iValor < 104)
                            {
                                oEnumRango = EnumRango.Intensidad.Baja;
                            }
                            else if (iValor >= 104)
                            {
                                oEnumRango = EnumRango.Intensidad.Nula;
                            }
                            break;
                        case "3G":
                            if (iValor < 85)
                            {
                                oEnumRango = EnumRango.Intensidad.Alta;
                            }
                            else if (iValor < 100)
                            {
                                oEnumRango = EnumRango.Intensidad.Media;
                            }
                            else if (iValor < 107)
                            {
                                oEnumRango = EnumRango.Intensidad.Baja;
                            }
                            else if (iValor >= 107)
                            {
                                oEnumRango = EnumRango.Intensidad.Nula;
                            }
                            break;
                        case "4G":
                            if (iValor < 94)
                            {
                                oEnumRango = EnumRango.Intensidad.Alta;
                            }
                            else if (iValor < 109)
                            {
                                oEnumRango = EnumRango.Intensidad.Media;
                            }
                            else if (iValor < 121)
                            {
                                oEnumRango = EnumRango.Intensidad.Baja;
                            }
                            else if (iValor >= 121)
                            {
                                oEnumRango = EnumRango.Intensidad.Nula;
                            }
                            break;
                        case "5G":
                            if (iValor < 94)
                            {
                                oEnumRango = EnumRango.Intensidad.Alta;
                            }
                            else if (iValor < 109)
                            {
                                oEnumRango = EnumRango.Intensidad.Media;
                            }
                            else if (iValor < 121)
                            {
                                oEnumRango = EnumRango.Intensidad.Baja;
                            }
                            else if (iValor >= 121)
                            {
                                oEnumRango = EnumRango.Intensidad.Nula;
                            }
                            break;
                        default:
                            if (iValor < 94)
                            {
                                oEnumRango = EnumRango.Intensidad.Alta;
                            }
                            else if (iValor < 109)
                            {
                                oEnumRango = EnumRango.Intensidad.Media;
                            }
                            else if (iValor < 121)
                            {
                                oEnumRango = EnumRango.Intensidad.Baja;
                            }
                            else if (iValor >= 121)
                            {
                                oEnumRango = EnumRango.Intensidad.Nula;
                            }

                            break;
                    }
                }
                else
                {
                    oEnumRango =EnumRango.Intensidad.Desconocido;
                }
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("enumRango", "calcularRangoIntensidad", ex.Message, true);
            }

            return oEnumRango;
        }

        public static EnumRango.VelocidadBajada calcularRangoVelocidadBajada(decimal? dValor, string sCategoria)
        {
            EnumRango.VelocidadBajada oEnumRango = EnumRango.VelocidadBajada.Desconocido;

            try
            {
                if (dValor.HasValue)
                {
                    int iValor = (int)dValor;

                    /*if (sCategoria == Constantes.RED_CABLEADA)
                    {
                        if (iValor < 20)
                        {
                            oEnumRango = EnumRango.VelocidadBajada.MuyBaja;
                        }
                        else if (iValor < 50)
                        {
                            oEnumRango = EnumRango.VelocidadBajada.Baja;
                        }
                        else if (iValor < 300)
                        {
                            oEnumRango = EnumRango.VelocidadBajada.Media;
                        }
                        else if (iValor < 600)
                        {
                            oEnumRango = EnumRango.VelocidadBajada.Alta;
                        }
                        else if (iValor >= 600)
                        {
                            oEnumRango = EnumRango.VelocidadBajada.MuyAlta;
                        }
                    }
                    else // por defecto umbral de red movil
                    {
                        if (iValor < 1)
                        {
                            oEnumRango = EnumRango.VelocidadBajada.MuyBaja;
                        }
                        else if (iValor < 5)
                        {
                            oEnumRango = EnumRango.VelocidadBajada.Baja;
                        }
                        else if (iValor < 30)
                        {
                            oEnumRango = EnumRango.VelocidadBajada.Media;
                        }
                        else if (iValor < 100)
                        {
                            oEnumRango = EnumRango.VelocidadBajada.Alta;
                        }
                        else if (iValor >= 100)
                        {
                            oEnumRango = EnumRango.VelocidadBajada.MuyAlta;
                        }
                    }*/

                    if (sCategoria == Constantes.RED_CABLEADA)
                    {
                        if (iValor < 30)
                        {
                            oEnumRango = EnumRango.VelocidadBajada.Baja;
                        }
                        else if (iValor < 100)
                        {
                            oEnumRango = EnumRango.VelocidadBajada.Media;
                        }
                        else if (iValor < 300)
                        {
                            oEnumRango = EnumRango.VelocidadBajada.Alta;
                        }
                        else if (iValor >= 300)
                        {
                            oEnumRango = EnumRango.VelocidadBajada.MuyAlta;
                        }
                    }
                    else // por defecto umbral de red movil
                    {
                        if (iValor < 1)
                        {
                            oEnumRango = EnumRango.VelocidadBajada.Baja;
                        }
                        else if (iValor < 5)
                        {
                            oEnumRango = EnumRango.VelocidadBajada.Media;
                        }
                        else if (iValor < 30)
                        {
                            oEnumRango = EnumRango.VelocidadBajada.Alta;
                        }
                        else if (iValor >= 30)
                        {
                            oEnumRango = EnumRango.VelocidadBajada.MuyAlta;
                        }
                    }
                }
                else
                {
                    oEnumRango = EnumRango.VelocidadBajada.Desconocido;
                }
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("enumRango", "calcularRangoVelocidadBajada", ex.Message, true);
            }

            return oEnumRango;
        }

        public static EnumRango.VelocidadSubida calcularRangoVelocidadSubida(decimal? dValor, string sCategoria)
        {
            EnumRango.VelocidadSubida oEnumRango = EnumRango.VelocidadSubida.Desconocido;

            try
            {
                if (dValor.HasValue)
                {
                    int iValor = (int)dValor;

                    /*if (sCategoria == Constantes.RED_CABLEADA)
                    {
                        if (iValor < 1)
                        {
                            oEnumRango = EnumRango.VelocidadSubida.MuyBaja;
                        }
                        else if (iValor < 20)
                        {
                            oEnumRango = EnumRango.VelocidadSubida.Baja;
                        }
                        else if (iValor < 50)
                        {
                            oEnumRango = EnumRango.VelocidadSubida.Media;
                        }
                        else if (iValor < 100)
                        {
                            oEnumRango = EnumRango.VelocidadSubida.Alta;
                        }
                        else if (iValor >= 100)
                        {
                            oEnumRango = EnumRango.VelocidadSubida.MuyAlta;
                        }
                    }
                    else // por defecto umbral de red movil
                    {
                        if (iValor < 0.5)
                        {
                            oEnumRango = EnumRango.VelocidadSubida.MuyBaja;
                        }
                        else if (iValor < 3)
                        {
                            oEnumRango = EnumRango.VelocidadSubida.Baja;
                        }
                        else if (iValor < 10)
                        {
                            oEnumRango = EnumRango.VelocidadSubida.Media;
                        }
                        else if (iValor < 30)
                        {
                            oEnumRango = EnumRango.VelocidadSubida.Alta;
                        }
                        else if (iValor >= 30)
                        {
                            oEnumRango = EnumRango.VelocidadSubida.MuyAlta;
                        }
                    } */

                    if (sCategoria == Constantes.RED_CABLEADA)
                    {
                        if (iValor < 30)
                        {
                            oEnumRango = EnumRango.VelocidadSubida.Baja;
                        }
                        else if (iValor < 100)
                        {
                            oEnumRango = EnumRango.VelocidadSubida.Media;
                        }
                        else if (iValor < 300)
                        {
                            oEnumRango = EnumRango.VelocidadSubida.Alta;
                        }
                        else if (iValor >= 300)
                        {
                            oEnumRango = EnumRango.VelocidadSubida.MuyAlta;
                        }
                    }
                    else // por defecto umbral de red movil
                    {
                        if (iValor < 0.5)
                        {
                            oEnumRango = EnumRango.VelocidadSubida.Baja;
                        }
                        else if (iValor < 3)
                        {
                            oEnumRango = EnumRango.VelocidadSubida.Media;
                        }
                        else if (iValor < 10)
                        {
                            oEnumRango = EnumRango.VelocidadSubida.Alta;
                        }
                        else if (iValor >= 10)
                        {
                            oEnumRango = EnumRango.VelocidadSubida.MuyAlta;
                        }
                    }
                }
                else
                {
                    oEnumRango = EnumRango.VelocidadSubida.Desconocido;
                }
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("enumRango", "calcularRangoVelocidadSubida", ex.Message, true);
            }

            return oEnumRango;
        }

        public static EnumRango.Latencia calcularRangoLatencia(decimal? dValor, string sCategoria)
        {
            EnumRango.Latencia oEnumRango = EnumRango.Latencia.Desconocido;

            try
            {
                if (dValor.HasValue)
                {
                    int iValor = (int)dValor;    
                    
                    if (sCategoria == Constantes.RED_CABLEADA)
                    {
                        if (iValor < 20)
                        {
                            oEnumRango = EnumRango.Latencia.MuyBaja;
                        }
                        else if (iValor < 60)
                        {
                            oEnumRango = EnumRango.Latencia.Baja;
                        }
                        else if (iValor < 100)
                        {
                            oEnumRango = EnumRango.Latencia.Media;
                        }
                        else if (iValor < 250)
                        {
                            oEnumRango = EnumRango.Latencia.Alta;
                        }
                        else if (iValor >= 250)
                        {
                            oEnumRango = EnumRango.Latencia.MuyAlta;
                        }
                    }
                    else // por defecto umbral de red movil
                    {
                        if (iValor < 35)
                        {
                            oEnumRango = EnumRango.Latencia.MuyBaja;
                        }
                        else if (iValor < 60)
                        {
                            oEnumRango = EnumRango.Latencia.Baja;
                        }
                        else if (iValor < 100)
                        {
                            oEnumRango = EnumRango.Latencia.Media;
                        }
                        else if (iValor < 250)
                        {
                            oEnumRango = EnumRango.Latencia.Alta;
                        }
                        else if (iValor >= 250)
                        {
                            oEnumRango = EnumRango.Latencia.MuyAlta;
                        }
                    }
                }
                else
                {
                    oEnumRango = EnumRango.Latencia.Desconocido;
                }
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("enumRango", "calcularRangoLatencia", ex.Message, true);
            }

            return oEnumRango;
        }

        public static int obtenerIconoRangoIntensidad(EnumRango.Intensidad oEnumRango)
        {
            int iValorIcono = 0;

            try
            {
                switch (oEnumRango) {
                    case EnumRango.Intensidad.Desconocido:

                        iValorIcono = 0;
                        break;
                    /*case EnumRango.Intensidad.SinSenyal:

                        iValorIcono = 0;
                        break;
                    case EnumRango.Intensidad.MuyBaja:

                        iValorIcono = 1;
                        break;*/
                    case EnumRango.Intensidad.Nula:

                        iValorIcono = 1;
                        break;
                    case EnumRango.Intensidad.Baja:

                        iValorIcono = 2;
                        break;
                    case EnumRango.Intensidad.Media:

                        iValorIcono = 3;
                        break;
                    case EnumRango.Intensidad.Alta:

                        iValorIcono = 4;
                        break;
                    /*case EnumRango.Intensidad.MuyAlta:

                        iValorIcono = 5;
                        break;*/
                    default:

                        iValorIcono = 0;
                        break;
                }
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("helper", "obtenerIconoRangoIntensidad", ex.Message, true);
            }

            return iValorIcono;
        }

        public static int obtenerIconoRangoVelocidadBajada(EnumRango.VelocidadBajada oEnumRango)
        {
            int iValorIcono = 0;

            try
            {
                switch (oEnumRango)
                {
                    case EnumRango.VelocidadBajada.Desconocido:

                        iValorIcono = 0;
                        break;
                    /*case EnumRango.VelocidadBajada.SinSenyal:

                        iValorIcono = 0;
                        break;*/
                    case EnumRango.VelocidadBajada.Baja:

                        iValorIcono = 1;
                        break;
                    case EnumRango.VelocidadBajada.Media:

                        iValorIcono = 2;
                        break;
                    case EnumRango.VelocidadBajada.Alta:

                        iValorIcono = 3;
                        break;
                    case EnumRango.VelocidadBajada.MuyAlta:

                        iValorIcono = 4;
                        break;
                    /*case EnumRango.VelocidadBajada.MuyAlta:

                        iValorIcono = 5;
                        break;*/
                    default:

                        iValorIcono = 0;
                        break;
                }
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("helper", "obtenerIconoRangoVelocidadBajada", ex.Message, true);
            }

            return iValorIcono;
        }

        public static int obtenerIconoRangoVelocidadSubida(EnumRango.VelocidadSubida oEnumRango)
        {
            int iValorIcono = 0;

            try
            {
                switch (oEnumRango)
                {
                    case EnumRango.VelocidadSubida.Desconocido:

                        iValorIcono = 0;
                        break;
                    /*case EnumRango.VelocidadSubida.SinSenyal:

                        iValorIcono = 0;
                        break;*/
                    case EnumRango.VelocidadSubida.Baja:

                        iValorIcono = 1;
                        break;
                    case EnumRango.VelocidadSubida.Media:

                        iValorIcono = 2;
                        break;
                    case EnumRango.VelocidadSubida.Alta:

                        iValorIcono = 3;
                        break;
                    case EnumRango.VelocidadSubida.MuyAlta:

                        iValorIcono = 4;
                        break;
                    /*case EnumRango.VelocidadSubida.MuyAlta:

                        iValorIcono = 5;
                        break;*/
                    default:

                        iValorIcono = 0;
                        break;
                }
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("helper", "obtenerIconoRangoVelocidadSubida", ex.Message, true);
            }

            return iValorIcono;
        }
        public static int obtenerRangoIconoLatencia(EnumRango.Latencia oEnumRango)
        {
            int iValorIcono = 0;

            try
            {
                switch (oEnumRango)
                {
                    case EnumRango.Latencia.Desconocido:

                        iValorIcono = 0;
                        break;
                    /*case EnumRango.Latencia.SinSenyal:

                        iValorIcono = 0;
                        break;*/
                    case EnumRango.Latencia.MuyBaja:

                        iValorIcono = 5;
                        break;
                    case EnumRango.Latencia.Baja:

                        iValorIcono = 4;
                        break;
                    case EnumRango.Latencia.Media:

                        iValorIcono = 3;
                        break;
                    case EnumRango.Latencia.Alta:

                        iValorIcono = 2;
                        break;
                    case EnumRango.Latencia.MuyAlta:

                        iValorIcono = 1;
                        break;
                    default:

                        iValorIcono = 0;
                        break;
                }
            }
            catch (Exception ex)
            {
                Helper.VolcarAConsola("helper", "obtenerRangoIconoLatencia", ex.Message, true);
            }

            return iValorIcono;
        }
    }
}
