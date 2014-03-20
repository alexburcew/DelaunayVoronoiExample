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
            var dataset = DataSet.Open(@"‪C:\Users\Alexandr.Burtsev\Downloads\ghcn_slice_3251.nc");
            var lon = dataset.GetData<Single[]>("lon");
            var lat = dataset.GetData<Single[]>("lat");
            var t = dataset.GetData<int[]>("temp");

            for (int i = 0; i < t.Length; i++)
            {
                if (t[i] != -9999)
                {
                    vertices.Add(new Vertex(lon[i], lat[i], t[i]));
                }
            }

            var fh = DateTime.Now;
            Delaunay_Voronoi dv = new Delaunay_Voronoi(vertices, true);

            var tim = DateTime.Now;
            List<Vertex> f = dv.NatNearestInterpolation(-110, 30, -50, -10, 100, 100, true, true, true);
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
                uncert2[y] = (Single)u.GetR_LV;
                y++;
            }


            dataset.Add<Single[]>("CornInterpolation");
            dataset.Add<Single[]>("UncertCornInterpolation");
            dataset.Add<Single[]>("lon2");
            dataset.Add<Single[]>("lat2");
            dataset.PutData<Single[]>("lon2", lon2);
            dataset.PutData<Single[]>("lat2", lat2);
            dataset.PutData<Single[]>("CornInterpolation", temp2);
            dataset.PutData<Single[]>("UncertCornInterpolation", uncert2);

            Console.WriteLine("ok");
            dataset.View();
        }
    }
}
