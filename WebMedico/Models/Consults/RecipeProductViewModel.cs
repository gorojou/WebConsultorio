using IFarmacias.Models.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebMedico.Models.Consults
{
    public class RecipeProductViewModel
    {
        public int? Id { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public string MedicationName { get; set; }
        [Required]
        public int? Qtd { get; set; }
        public string Posology { get; set; }
        public static RecipeProductsResponse GetBase(RecipeProductViewModel vmodel)
        {
            RecipeProductsResponse model = new RecipeProductsResponse();
            model.Code = vmodel.Code;
            model.MedicationName = vmodel.MedicationName;
            model.Posology = vmodel.Posology;
            model.Qtd = vmodel.Qtd;

            return model;
        }

        public static List<RecipeProductsResponse> GetBaseList(List<RecipeProductViewModel> vmodel)
        {
            List<RecipeProductsResponse> model = new List<RecipeProductsResponse>();
            
            foreach (var item in vmodel)
            {
                model.Add(GetBase(item));
            }

            return model;
        }

        public static RecipeProductViewModel Parse(RecipeProductsResponse model)
        {
            RecipeProductViewModel vmodel = new RecipeProductViewModel();
            vmodel.Code = model.Code;
            vmodel.MedicationName = model.MedicationName;
            vmodel.Posology = model.Posology;
            vmodel.Qtd = model.Qtd;

            return vmodel;
        }

        public static List<RecipeProductViewModel> ParseList(List<RecipeProductsResponse> model)
        {
            List<RecipeProductViewModel> vmodel = new List<RecipeProductViewModel>();

            foreach (var item in model)
            {
                vmodel.Add(Parse(item));
            }

            return vmodel;
        }
    }
}