// TicketDesk - Attribution notice
// Contributor(s):
//
//      Stephen Redd (https://github.com/stephenredd)
//
// This file is distributed under the terms of the Microsoft Public 
// License (Ms-PL). See http://opensource.org/licenses/MS-PL
// for the complete terms of use. 
//
// For any distribution that contains code from this file, this notice of 
// attribution must remain intact, and a copy of the license must be 
// provided to the recipient.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using TicketDesk.Domain;
using TicketDesk.Domain.Model;
using TicketDesk.Web.Client.Models;

namespace TicketDesk.Web.Client.Controllers
{
    [RoutePrefix("tickets")]
    [Route("{action=index}")]
    [TdAuthorize(Roles = "TdInternalUsers,TdHelpDeskUsers,TdAdministrators")]
    public class TicketCenterController : Controller
    {
        private TdDomainContext Context { get; set; }
        public TicketCenterController(TdDomainContext context)
        {
            Context = context;
        }

        [Route("reset-user-lists")]
        public async Task<ActionResult> ResetUserLists()
        {
            var uId = Context.SecurityProvider.CurrentUserId;
            await Context.UserSettingsManager.ResetAllListSettingsForUserAsync(uId);
            var x = await Context.SaveChangesAsync();
            return RedirectToAction("Index");

        }

        // GET: TicketCenter
        [Route("{listName?}/{page:int?}")]
        public async Task<ActionResult> Index(int? page, string listName)
        {
            listName = listName ?? (Context.SecurityProvider.IsTdHelpDeskUser ? "unassigned" : "mytickets");
            var pageNumber = page ?? 1;

            var viewModel = await TicketCenterListViewModel.GetViewModelAsync(pageNumber, listName, Context, Context.SecurityProvider.CurrentUserId);//new TicketCenterListViewModel(listName, model, Context, User.Identity.GetUserId());

            return View(viewModel);
        }

        [Route("pageList/{listName=mytickets}/{page:int?}")]
        public async Task<ActionResult> PageList(int? page, string listName)
        {
            return await GetTicketListPartial(page, listName);
        }


        [Route("filterList/{listName=opentickets}/{page:int?}")]
        public async Task<PartialViewResult> FilterList(
            string listName,
            int pageSize,
            int projectId,
            string ticketStatus,
            string owner,
            string assignedTo)
        {
            string daterange = HttpContext.Request.Params.Get("daterange");
            DateTime toDate = Convert.ToDateTime("01/01/" + DateTime.Now.Year);
            DateTime fromDate = Convert.ToDateTime("12/31/" + DateTime.Now.Year);
            if (daterange != null && daterange.Length > 1)
            {
                List<string> lstTime = daterange.Split('-').ToList();
                toDate = Convert.ToDateTime(lstTime[0]);
                fromDate = Convert.ToDateTime(lstTime[1]);
            }
            var uId = Context.SecurityProvider.CurrentUserId;
            var userSetting = await Context.UserSettingsManager.GetSettingsForUserAsync(uId);
            if (userSetting != null && userSetting.ListSettings.Any())
            {
                for (int i = 0; i < userSetting.ListSettings.Count; i++)
                {
                    if (userSetting.ListSettings[i].ListName.ToLower().Equals(listName.ToString().ToLower()))
                    {
                        foreach (var item in userSetting.ListSettings[i].FilterColumns)
                        {
                            if (item.ColumnName.Equals("ToDate"))
                            {
                                item.ColumnValue = toDate.ToString("MM/dd/yyyy");
                            }
                            if (item.ColumnName.Equals("FromDate"))
                            {
                                item.ColumnValue = fromDate.ToString("MM/dd/yyyy");
                            }
                        }
                    }
                }
            }
            using (var tempCtx = new TdDomainContext())
            {
                await tempCtx.UserSettingsManager.AddOrUpdateSettingsForUser(userSetting);
                await tempCtx.SaveChangesAsync();
            }
            var currentListSetting = userSetting.GetUserListSettingByName(listName);

            currentListSetting.ModifyFilterSettings(pageSize, projectId, ticketStatus, owner, assignedTo, toDate, fromDate);//Tứ bổ sung projectId toDate, fromDate cho filter 13/03/2020
            
            await Context.SaveChangesAsync();

            return await GetTicketListPartial(null, listName);

        }

        [Route("sortList/{listName=opentickets}/{page:int?}")]
        public async Task<PartialViewResult> SortList(
            int? page,
            string listName,
            string columnName,
            bool isMultiSort = false)
        {
            var uId = Context.SecurityProvider.CurrentUserId;
            var userSetting = await Context.UserSettingsManager.GetSettingsForUserAsync(uId);
            var currentListSetting = userSetting.GetUserListSettingByName(listName);

            var sortCol = currentListSetting.SortColumns.SingleOrDefault(sc => sc.ColumnName == columnName);

            if (isMultiSort)
            {
                if (sortCol != null)// column already in sort, remove from sort
                {
                    if (currentListSetting.SortColumns.Count > 1)//only remove if there are more than one sort
                    {
                        currentListSetting.SortColumns.Remove(sortCol);
                    }
                }
                else// column not in sort, add to sort
                {
                    currentListSetting.SortColumns.Add(new UserTicketListSortColumn(columnName, ColumnSortDirection.Ascending));
                }
            }
            else
            {
                if (sortCol != null)// column already in sort, just flip direction
                {
                    sortCol.SortDirection = (sortCol.SortDirection == ColumnSortDirection.Ascending) ? ColumnSortDirection.Descending : ColumnSortDirection.Ascending;
                }
                else // column not in sort, replace sort with new simple sort for column
                {
                    currentListSetting.SortColumns.Clear();
                    currentListSetting.SortColumns.Add(new UserTicketListSortColumn(columnName, ColumnSortDirection.Ascending));
                }
            }

            await Context.SaveChangesAsync();

            return await GetTicketListPartial(page, listName);
        }



        private async Task<PartialViewResult> GetTicketListPartial(int? page, string listName)
        {
            var pageNumber = page ?? 1;

            var viewModel = await TicketCenterListViewModel.GetViewModelAsync(pageNumber, listName, Context, Context.SecurityProvider.CurrentUserId);
            return PartialView("_TicketList", viewModel);

        }
    }
}
