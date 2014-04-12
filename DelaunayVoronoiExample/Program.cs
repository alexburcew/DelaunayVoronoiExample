using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Delaunay_Voronoi_Library;
using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;

namespace Lipshits
{
    class Program
    {
        static void Main(string[] args)
        {
            var vertices = new List<Vertex>();
            var dataset = DataSet.Open(@"‪C:\Users\Alexandr.Burtsev\Downloads\GHCN_monthly.nc");
            var t = dataset.GetData<double[,]>("temp_mean");
            var lat = dataset.GetData<Single[]>("lat");
            var lon = dataset.GetData<Single[]>("lon");
            var s = dataset.GetData<double[,]>("temp_sigma");

            for (int i = 0; i < t.GetLength(1); i++)
            {
                if (t[1, i] != -9999)
                {
                    vertices.Add(new Vertex(lon[i], lat[i], t[1, i], s[1, i]));
                }
            }

            var fh = DateTime.Now;
            Delaunay_Voronoi dv = new Delaunay_Voronoi(vertices, true);

            var tim = DateTime.Now;
            var f = dv.crossadd(true);

            int kvgood = 0;
            double kmax = 0.0;
            foreach (var ry in f)
            {
                double param2 = Math.Abs(ry.CrossValue - ry.Value);
                if (param2 < ry.Sigma + ry.GetUncertainty) kvgood += 1; else if (!double.IsNaN(-(ry.Sigma + ry.GetUncertainty) + param2)) kmax = Math.Max(kmax, -(ry.Sigma + ry.GetUncertainty) + param2);
                //ry.Value = ry.CrossValue;
                ry.Value = ry.Sigma + ry.GetUncertainty;
            }



            Console.WriteLine("Interpolation: {0}", DateTime.Now - tim);
            Console.WriteLine("Total time: {0}", DateTime.Now - fh);

            string stname = "a" + (-DateTime.Now.ToBinary()).ToString();
            dataset.Clone("‪C:/Users/Alexandr.Burtsev/Downloads/" + stname + ".nc");
            dataset = DataSet.Open("‪C:/Users/Alexandr.Burtsev/Downloads/" + stname + ".nc");

            int lenght = f.Count;
            var lon2 = new Single[lenght];
            var lat2 = new Single[lenght];
            var temp2 = new Single[lenght];
            var uncert2 = new Single[lenght];
            int y = 0;

            foreach (var u in f)
            {
                lon2[y] = (Single)u.Longitude;
                lat2[y] = (Single)u.Latitude;
                temp2[y] = (Single)u.Value;
                uncert2[y] = (Single)u.Sigma;
                y++;
            }

            dataset.Add<Single[]>("InterpolationValue");
            dataset.Add<Single[]>("InterpolationUncert");
            dataset.Add<Single[]>("lon2");
            dataset.Add<Single[]>("lat2");
            dataset.PutData<Single[]>("lon2", lon2);
            dataset.PutData<Single[]>("lat2", lat2);
            dataset.PutData<Single[]>("InterpolationValue", temp2);
            dataset.PutData<Single[]>("InterpolationUncert", uncert2);
            Console.WriteLine("kvgood: {0} - {1}.", 100.0 * kvgood / f.Count, kmax);
            Console.WriteLine("ok");
            dataset.View();

        }
    }
}
