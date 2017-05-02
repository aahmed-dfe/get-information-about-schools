﻿using Autofac;
using Edubase.Common;
using Edubase.Services.Domain;
using FluentValidation;
using FluentValidation.Resources;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace Edubase.Web.UI
{
    using FluentValidation.Internal;
    using FluentValidation.Resources;
    using FluentValidation.Validators;
    using MoreLinq;

    public static class LangExt
    {
        public static IEnumerable<SelectListItem> ToSelectList(this IEnumerable<LookupDto> items, int? currentId) 
            => items.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = currentId.HasValue && currentId.Value == x.Id });

        /// <summary>
        /// Adds an item to the list if it's not already in there.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static List<int> AddUniqueMutable(this List<int> list, int item)
        {
            if (!list.Contains(item)) list.Add(item);
            return list;
        }

        public static List<int> AddUnique(this List<int> list, int item)
        {
            var retVal = new List<int>(list);
            if (!retVal.Contains(item)) retVal.Add(item);
            return retVal;
        }

        public static void AddModelError<TModel, TProperty>(this TModel source,
                                                    Expression<Func<TModel, TProperty>> ex,
                                                    string message,
                                                    ModelStateDictionary modelState)
        {
            var key = ExpressionHelper.GetExpressionText(ex);
            modelState.AddModelError(key, message);
        }

        /// <summary>
        /// Adds one or more items under the same key and returns the new collection (incoming ref is treated as immutable).
        /// Only distinct values will be added. If the key/value combination exists, it's ignored.
        /// </summary>
        /// <param name="nvc"></param>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static NameValueCollection AddIfNonExistent(this NameValueCollection nvc, string key, params object[] values)
        {
            nvc = HttpUtility.ParseQueryString(nvc.ToString());
            Guard.IsNotNull(key.Clean(), () => new ArgumentNullException(nameof(key)));
            Guard.IsNotNull(values, () => new ArgumentNullException(nameof(values)));
            var items = values.Select(x => x?.ToString().Clean()).Distinct();
            foreach (var value in items)
            {
                var data = nvc.GetValues(key);
                if (data == null || (data != null && !data.Contains(value))) nvc.Add(key, value);
            }
            return nvc;
        }

        /// <summary>
        /// Adds a message that will appear in the validation summary, in addition to the field-level message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="rule"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static IRuleBuilderOptions<T, TProperty> WithSummaryMessage<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule, string message) => rule.WithState(x => message);

        public static IRuleBuilderOptions<T, TProperty> WithSummaryMessage<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule, Func<T, object> messageProvider) => rule.WithState(messageProvider);

    }
}