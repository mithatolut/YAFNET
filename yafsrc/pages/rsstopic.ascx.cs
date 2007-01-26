/* Yet Another Forum.net
 * Copyright (C) 2003 Bj�rnar Henden
 * http://www.yetanotherforum.net/
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 */

using System;
using System.Xml;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using YAF.Classes.Utils;
using YAF.Classes.Data;

namespace YAF.Pages // YAF.Pages
{
	/// <summary>
	/// Summary description for rss.
	/// </summary>
	public partial class rsstopic : YAF.Classes.Base.ForumPage
	{
		public rsstopic()
			: base( "RSSTOPIC" )
		{
		}

		protected void Page_Load( object sender, System.EventArgs e )
		{
			// Put user code to initialize the page here
			RssFeed rf = new RssFeed();

			XmlTextWriter writer = new XmlTextWriter( Response.OutputStream, System.Text.Encoding.UTF8 );

			writer.Formatting = Formatting.Indented;
			rf.WriteRSSPrologue( writer );

			// Usage rf.AddRSSItem(writer, "Item Title", "http://test.com", "This is a test item");

			switch ( Request.QueryString ["pg"] )
			{
				case "latestposts":
					if ( !PageContext.ForumReadAccess )
						yaf_BuildLink.AccessDenied();
					using ( DataTable dt = YAF.Classes.Data.DB.topic_latest( PageContext.PageBoardID, 7, PageContext.PageUserID ) )
					{
						foreach ( DataRow row in dt.Rows )
							rf.AddRSSItem( writer, row ["Subject"].ToString(), yaf_ForumInfo.ServerURL + yaf_BuildLink.GetLink( ForumPages.posts, "t={0}", Request.QueryString ["t"] ), row ["Message"].ToString(), Convert.ToDateTime( row ["Posted"] ).ToString( "r" ) );
					}
					break;
				case "latestannouncements":
					if ( !PageContext.ForumReadAccess )
						yaf_BuildLink.AccessDenied();
					using ( DataTable dt = YAF.Classes.Data.DB.topic_announcements( PageContext.PageBoardID, 7, PageContext.PageUserID ) )
					{
						foreach ( DataRow row in dt.Rows )
							rf.AddRSSItem( writer, row ["Subject"].ToString(), yaf_ForumInfo.ServerURL + yaf_BuildLink.GetLink( ForumPages.posts, "t={0}", Request.QueryString ["t"] ), row ["Message"].ToString(), Convert.ToDateTime( row ["Posted"] ).ToString( "r" ) );
					}
					break;
				case "posts":
					if ( !PageContext.ForumReadAccess )
						yaf_BuildLink.AccessDenied();

					if ( Request.QueryString ["t"] != null )
					{
						using ( DataTable dt = YAF.Classes.Data.DB.post_list( PageContext.PageTopicID, 1, PageContext.BoardSettings.ShowDeletedMessages ) )
						{
							foreach ( DataRow row in dt.Rows )
								rf.AddRSSItem( writer, row ["Subject"].ToString(), yaf_ForumInfo.ServerURL + yaf_BuildLink.GetLink( ForumPages.posts, "t={0}", Request.QueryString ["t"] ), row ["Message"].ToString(), Convert.ToDateTime( row ["Posted"] ).ToString( "r" ) );
						}
					}

					break;
				case "forum":
					using ( DataTable dt = YAF.Classes.Data.DB.forum_listread( PageContext.PageBoardID, PageContext.PageUserID, null, null ) )
					{
						foreach ( DataRow row in dt.Rows )
						{
							if ( row ["LastTopicID"] != DBNull.Value )
							{
								rf.AddRSSItem( writer, row ["Forum"].ToString(), yaf_ForumInfo.ServerURL + yaf_BuildLink.GetLink( ForumPages.topics, "f={0}", row ["ForumID"] ), row ["Description"].ToString() );
							}
							else
							{
								rf.AddRSSItem( writer, row ["Forum"].ToString(), yaf_ForumInfo.ServerURL + yaf_BuildLink.GetLink( ForumPages.topics, "f={0}", row ["ForumID"] ), row ["Description"].ToString() );
							}
						}
					}
					break;
				case "topics":
					if ( !PageContext.ForumReadAccess )
						yaf_BuildLink.AccessDenied();

					if ( Request.QueryString ["f"] != null )
					{
						string tSQL = "select Topic = a.Topic, TopicID = a.TopicID, Name = b.Name, Posted = a.Posted from yaf_Topic a, yaf_Forum b where a.ForumID=" + Request.QueryString ["f"] + " and b.ForumID = a.ForumID";
						using ( DataTable dt = YAF.Classes.Data.DBAccess.GetData( tSQL ) )
						{
							foreach ( DataRow row in dt.Rows )
							{
								rf.AddRSSItem( writer, row ["Topic"].ToString(), yaf_ForumInfo.ServerURL + yaf_BuildLink.GetLink( ForumPages.posts, "t={0}", row ["TopicID"] ), row ["Topic"].ToString(), Convert.ToDateTime( row ["Posted"] ).ToString( "r" ) );
							}
						}
					}

					break;
				case "active":
					using ( DataTable dt = YAF.Classes.Data.DB.topic_active( PageContext.PageBoardID, PageContext.PageUserID, DateTime.Now + TimeSpan.FromHours( -24 ), PageContext.Settings.CategoryID ) )
					{
						foreach ( DataRow row in dt.Rows )
						{
							rf.AddRSSItem( writer, row ["Subject"].ToString(), yaf_ForumInfo.ServerURL + yaf_BuildLink.GetLink( ForumPages.posts, "t={0}", row ["LinkTopicID"] ), row ["Subject"].ToString() );
						}
					}
					break;
				default:
					break;
			}

			rf.WriteRSSClosing( writer );
			writer.Flush();

			writer.Close();

			Response.ContentEncoding = System.Text.Encoding.UTF8;
			Response.ContentType = "text/xml";
			Response.Cache.SetCacheability( HttpCacheability.Public );

			Response.End();
		}

		#region Web Form Designer generated code
		override protected void OnInit( EventArgs e )
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit( e );
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}
		#endregion
	}
}
