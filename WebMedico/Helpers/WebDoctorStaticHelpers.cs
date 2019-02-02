using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Linq.Expressions;
using System.Web.Routing;
namespace WebMedico.Helpers
{
    public enum HtmlAttributesPossibleConfigurationValues
    {
        labeltext,iprefix,texboxclass, textboxtype, onfocusout, onkeypress, formid
    }
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString TexboxBlockFor<T, TValue>(this HtmlHelper<T> htmlhelper,
                                                  Expression<System.Func<T, TValue>> expression,
                                                  Dictionary<HtmlAttributesPossibleConfigurationValues, string> htmlAttributes = null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlhelper.ViewData);
            string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
            string propertyName = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            string labeltext = string.Empty; string iprefix = string.Empty; string textboxclass = string.Empty; string texboxtype = "text"; string onfocusout = string.Empty; string onkeypress = string.Empty; string formid = string.Empty;
            if (htmlAttributes != null)
            {
                if (htmlAttributes.ContainsKey(HtmlAttributesPossibleConfigurationValues.labeltext)) { labeltext = htmlAttributes[HtmlAttributesPossibleConfigurationValues.labeltext]; }
                if (htmlAttributes.ContainsKey(HtmlAttributesPossibleConfigurationValues.texboxclass)) { textboxclass = htmlAttributes[HtmlAttributesPossibleConfigurationValues.texboxclass]; }
                if (htmlAttributes.ContainsKey(HtmlAttributesPossibleConfigurationValues.iprefix)) { iprefix = htmlAttributes[HtmlAttributesPossibleConfigurationValues.iprefix]; }
                if (htmlAttributes.ContainsKey(HtmlAttributesPossibleConfigurationValues.textboxtype)) { texboxtype = htmlAttributes[HtmlAttributesPossibleConfigurationValues.textboxtype]; }
                if (htmlAttributes.ContainsKey(HtmlAttributesPossibleConfigurationValues.onfocusout)) { onfocusout = htmlAttributes[HtmlAttributesPossibleConfigurationValues.onfocusout]; }
                if (htmlAttributes.ContainsKey(HtmlAttributesPossibleConfigurationValues.onkeypress)) { onkeypress = htmlAttributes[HtmlAttributesPossibleConfigurationValues.onkeypress]; }
                if (htmlAttributes.ContainsKey(HtmlAttributesPossibleConfigurationValues.formid)) { formid = htmlAttributes[HtmlAttributesPossibleConfigurationValues.formid]; }
            }
            TagBuilder div = new TagBuilder("div");
            div.Attributes.Add("class", "md-form");
            textboxclass = (textboxclass == "") ? "" : " " + textboxclass;
            MvcHtmlString textbox = htmlhelper.TextBoxFor(expression, propertyName, new { @class = "form-control" + textboxclass , @type = texboxtype, @onfocusout = onfocusout, @onkeypress = onkeypress });
            MvcHtmlString validationmessagefor = htmlhelper.ValidationMessageFor(expression, string.Empty, new { @class = "text-danger" });
            MvcHtmlString label = htmlhelper.LabelFor(expression, (labeltext == "") ? propertyName : labeltext, new { @class = "" });
            div.InnerHtml = iprefix + textbox.ToHtmlString() + validationmessagefor.ToHtmlString() + label.ToHtmlString();
            return MvcHtmlString.Create(div.ToString(TagRenderMode.Normal));
        }
        public static MvcHtmlString PasswordBlockFor<T, TValue>(this HtmlHelper<T> htmlhelper,
                                                 Expression<System.Func<T, TValue>> expression, bool eyeopen = false,
                                                 Dictionary<HtmlAttributesPossibleConfigurationValues, string> htmlAttributes = null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlhelper.ViewData);
            string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
            string propertyName = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            string labeltext = string.Empty; string iprefix = string.Empty; string textboxclass = string.Empty; string texboxtype = "text"; string onfocusout = string.Empty; string onkeypress = string.Empty; string formid = string.Empty;
            if (htmlAttributes != null)
            {
                if (htmlAttributes.ContainsKey(HtmlAttributesPossibleConfigurationValues.labeltext)) { labeltext = htmlAttributes[HtmlAttributesPossibleConfigurationValues.labeltext]; }
                if (htmlAttributes.ContainsKey(HtmlAttributesPossibleConfigurationValues.texboxclass)) { textboxclass = htmlAttributes[HtmlAttributesPossibleConfigurationValues.texboxclass]; }
                if (htmlAttributes.ContainsKey(HtmlAttributesPossibleConfigurationValues.iprefix)) { iprefix = htmlAttributes[HtmlAttributesPossibleConfigurationValues.iprefix]; }
                if (htmlAttributes.ContainsKey(HtmlAttributesPossibleConfigurationValues.textboxtype)) { texboxtype = htmlAttributes[HtmlAttributesPossibleConfigurationValues.textboxtype]; }
                if (htmlAttributes.ContainsKey(HtmlAttributesPossibleConfigurationValues.onfocusout)) { onfocusout = htmlAttributes[HtmlAttributesPossibleConfigurationValues.onfocusout]; }
                if (htmlAttributes.ContainsKey(HtmlAttributesPossibleConfigurationValues.onkeypress)) { onkeypress = htmlAttributes[HtmlAttributesPossibleConfigurationValues.onkeypress]; }
                if (htmlAttributes.ContainsKey(HtmlAttributesPossibleConfigurationValues.formid)) { formid = htmlAttributes[HtmlAttributesPossibleConfigurationValues.formid]; }
            }
            TagBuilder div = new TagBuilder("div");
            div.Attributes.Add("class", "md-form");
            textboxclass = (textboxclass == "") ? "" : " " + textboxclass;
            MvcHtmlString password = htmlhelper.PasswordFor(expression, new { @class = "form-control" + textboxclass, @type = "password", @onfocusout = onfocusout, @onkeypress = onkeypress });
            MvcHtmlString validationmessagefor = htmlhelper.ValidationMessageFor(expression, string.Empty, new { @class = "text-danger" });
            MvcHtmlString label = htmlhelper.LabelFor(expression, (labeltext == "") ? propertyName : labeltext, new { @class = "" });
            string diveyes = (eyeopen) ? "<div id=\"eyeopen\" class=\"toggler-ico\" style=\"display:block;\"><svg fill=\"#D8D8D8\" x=\"0px\" y=\"0px\" width=\"22px\" height=\"22px\" viewBox=\"0 0 1024 1024\" preserveAspectRatio='xMinYMin'><path d=\"M512 192c-223.318 0-416.882 130.042-512 320 95.118 189.958 288.682 320 512 320 223.312 0 416.876-130.042 512-320-95.116-189.958-288.688-320-512-320zM764.45 361.704c60.162 38.374 111.142 89.774 149.434 150.296-38.292 60.522-89.274 111.922-149.436 150.296-75.594 48.218-162.89 73.704-252.448 73.704-89.56 0-176.858-25.486-252.452-73.704-60.158-38.372-111.138-89.772-149.432-150.296 38.292-60.524 89.274-111.924 149.434-150.296 3.918-2.5 7.876-4.922 11.86-7.3-9.96 27.328-15.41 56.822-15.41 87.596 0 141.382 114.616 256 256 256 141.382 0 256-114.618 256-256 0-30.774-5.452-60.268-15.408-87.598 3.978 2.378 7.938 4.802 11.858 7.302v0zM512 416c0 53.020-42.98 96-96 96s-96-42.98-96-96 42.98-96 96-96 96 42.982 96 96z\"></path></svg></div><div id=\"eyeclosed\" class=\"toggler-ico\" style=\"display:none;\"><svg fill=\"#D8D8D8\" x=\"0px\" y=\"0px\" width=\"22px\" height=\"22px\" viewBox=\"0 0 1024 1024\" preserveAspectRatio='xMinYMin'><path d=\"M945.942 14.058c-18.746-18.744-49.136-18.744-67.882 0l-202.164 202.164c-51.938-15.754-106.948-24.222-163.896-24.222-223.318 0-416.882 130.042-512 320 41.122 82.124 100.648 153.040 173.022 207.096l-158.962 158.962c-18.746 18.746-18.746 49.136 0 67.882 9.372 9.374 21.656 14.060 33.94 14.060s24.568-4.686 33.942-14.058l864-864c18.744-18.746 18.744-49.138 0-67.884zM416 320c42.24 0 78.082 27.294 90.92 65.196l-121.724 121.724c-37.902-12.838-65.196-48.68-65.196-90.92 0-53.020 42.98-96 96-96zM110.116 512c38.292-60.524 89.274-111.924 149.434-150.296 3.918-2.5 7.876-4.922 11.862-7.3-9.962 27.328-15.412 56.822-15.412 87.596 0 54.89 17.286 105.738 46.7 147.418l-60.924 60.924c-52.446-36.842-97.202-83.882-131.66-138.342z\"></path><path d=\"M768 442c0-27.166-4.256-53.334-12.102-77.898l-321.808 321.808c24.568 7.842 50.742 12.090 77.91 12.090 141.382 0 256-114.618 256-256z\"></path><path d=\"M830.026 289.974l-69.362 69.362c1.264 0.786 2.53 1.568 3.786 2.368 60.162 38.374 111.142 89.774 149.434 150.296-38.292 60.522-89.274 111.922-149.436 150.296-75.594 48.218-162.89 73.704-252.448 73.704-38.664 0-76.902-4.76-113.962-14.040l-76.894 76.894c59.718 21.462 123.95 33.146 190.856 33.146 223.31 0 416.876-130.042 512-320-45.022-89.916-112.118-166.396-193.974-222.026z\"></path></svg></div>" : string.Empty;
            string script = (eyeopen) ? "<script type=\"text/javascript\">" +
                              @"var ele = document.getElementById('" + propertyName + @"');
                                document.getElementById('eyeopen').onclick = function () {
                                    document.getElementById('eyeopen').style.display = 'none';
                                    document.getElementById('eyeclosed').style.display = 'block';
                                    ele.type = 'text';
                                }
                                document.getElementById('eyeclosed').onclick = function() {
                                    document.getElementById('eyeopen').style.display = 'block';
                                    document.getElementById('eyeclosed').style.display = 'none';
                                    ele.type = 'password';
                                }
                            </script>" : string.Empty;

            div.InnerHtml = iprefix + password.ToHtmlString() + validationmessagefor.ToHtmlString() + label.ToHtmlString() + diveyes + script;
            return MvcHtmlString.Create(div.ToString(TagRenderMode.Normal));
        }
        public static MvcHtmlString DateTimeBlockFor<T, TValue>(this HtmlHelper<T> htmlhelper,
                                                  Expression<System.Func<T, TValue>> expression,
                                                  Dictionary<HtmlAttributesPossibleConfigurationValues, string> htmlAttributes = null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlhelper.ViewData);
            string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
            string propertyName = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            string labeltext = string.Empty; string iprefix = string.Empty; string textboxclass = string.Empty; string texboxtype = "text"; string onfocusout = string.Empty; string onkeypress = string.Empty;string formid = "#formvalidate";
            if (htmlAttributes != null)
            {
                if (htmlAttributes.ContainsKey(HtmlAttributesPossibleConfigurationValues.labeltext)) { labeltext = htmlAttributes[HtmlAttributesPossibleConfigurationValues.labeltext]; }
                if (htmlAttributes.ContainsKey(HtmlAttributesPossibleConfigurationValues.texboxclass)) { textboxclass = htmlAttributes[HtmlAttributesPossibleConfigurationValues.texboxclass]; }
                if (htmlAttributes.ContainsKey(HtmlAttributesPossibleConfigurationValues.iprefix)) { iprefix = htmlAttributes[HtmlAttributesPossibleConfigurationValues.iprefix]; }
                if (htmlAttributes.ContainsKey(HtmlAttributesPossibleConfigurationValues.textboxtype)) { texboxtype = htmlAttributes[HtmlAttributesPossibleConfigurationValues.textboxtype]; }
                if (htmlAttributes.ContainsKey(HtmlAttributesPossibleConfigurationValues.onfocusout)) { onfocusout = htmlAttributes[HtmlAttributesPossibleConfigurationValues.onfocusout]; }
                if (htmlAttributes.ContainsKey(HtmlAttributesPossibleConfigurationValues.onkeypress)) { onkeypress = htmlAttributes[HtmlAttributesPossibleConfigurationValues.onkeypress]; }
                if (htmlAttributes.ContainsKey(HtmlAttributesPossibleConfigurationValues.formid)) { formid = htmlAttributes[HtmlAttributesPossibleConfigurationValues.formid]; }
            }
            TagBuilder div = new TagBuilder("div");
            div.Attributes.Add("class", "md-form");
            textboxclass = (textboxclass == "") ? "" : " " + textboxclass;
            MvcHtmlString textbox = htmlhelper.TextBoxFor(expression, propertyName, new { @class = "form-control datepicker" + textboxclass });
            string script = "<script>" + "$('#" + htmlFieldName.Split('.').Last() + "').change(function() {$('" + formid + "').data('validator').element('#' + $(this).attr('id'));});" + "</script>";
            MvcHtmlString validationmessagefor = htmlhelper.ValidationMessageFor(expression, string.Empty, new { @class = "text-danger" });
            MvcHtmlString label = htmlhelper.LabelFor(expression, (labeltext == "") ? propertyName : labeltext, new { @class = "" });
            div.InnerHtml = iprefix + textbox.ToHtmlString() + validationmessagefor.ToHtmlString() + script +  label.ToHtmlString();
            return MvcHtmlString.Create(div.ToString(TagRenderMode.Normal));
        }
    }
}
