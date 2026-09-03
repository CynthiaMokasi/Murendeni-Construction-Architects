# Project Prototype

This folder contains the initial prototype files for the Murendeni Construction Architects project.

The prototype will be updated as the website and mobile application designs are developed.

## Prototype Components

- Website prototype
- Mobile application prototype
- Initial design assets
- Supporting prototype files


# Murendeni Construction Architects - Web App Summary

This document explains what the app does and how it's built, in plain language. Think of it as a tour guide for your own project.

1. What this app actually is

It's one website with three different "sides" to it, depending on who's looking at it:

Public Website - anyone on the internet can see this. Home, About, Services, Portfolio, Contact.
Client Portal - a private area for your clients. They log in and see only their own projects and files.
Admin Portal - a private area for your staff (Admin, Sales, Designer). They log in and manage clients, projects, files, inquiries, and reports.

All three sides share one project in Visual Studio and one database in Azure. Nothing is duplicated - the same Client table, for example, is used by the Contact form, the Client Portal, and the Admin Portal.

2. The pieces, and what each one is for
Folder	What lives there	Plain-English job
Models/	Client.cs, Employee.cs, ProjectProfile.cs, Design.cs, Payment.cs, Inquiry.cs	Each file is a C# "shape" that matches one database table.
Data/ApplicationDbContext.cs	One big file	Tells the app exactly how those C# shapes map onto the real Azure SQL columns.
Helpers/PasswordHasher.cs	One file	Turns a password into a scrambled, safe-to-store version. Never store real passwords.
Pages/ (top level)	Home, About, Services, Contact	The public website.
Pages/Portfolio/	Index, Details	The public project gallery.
Pages/Account/	Register, Login, Logout	Shared login for both Clients and Staff.
Pages/Portal/	Dashboard, My Projects, Project Details, Account Settings, Change Password	The Client Portal. Locked to logged-in Clients only.
Pages/Admin/	Dashboard, Manage Clients, Manage Projects, Upload Design Files, Manage Portfolio, Manage Inquiries, Reports, Add Employee	The Admin Portal. Locked to logged-in staff only.
wwwroot/css/	site.css, admin.css	All the visual styling - colors, fonts, layout.
wwwroot/uploads/	Uploaded files land here (temporary solution, see Section 7)	

The pattern used on almost every page: a .cshtml.cs file does the thinking (loads data from the database, checks a form, saves changes), and the matching .cshtml file just displays whatever the .cshtml.cs file prepared. Once you understand one page, you basically understand them all.

3. How the database is organized

Six tables, and how they connect:

Client - someone who hires you for a project.
Employee - your staff. Has a Role: Admin, Designer, or Sales.
Profile (called ProjectProfile in the C# code, but really means "a Project") - one row per project. Belongs to one Client and one assigned Employee.
Design - one uploaded file (a floor plan, a 3D model, etc.), belonging to one Project.
Payment - a payment record tied to a Client, Project, and Design.
Inquiry - a message from the Contact form, or a follow-up from an existing client. Can exist with no Client attached yet (a "guest" inquiry), until someone converts it into a real account.
4. How logging in works

There's one login page (/Account/Login) for everyone - Clients and staff both type their email and password into the same form.

Behind the scenes:

The app first checks if that email belongs to a Client. If the password matches, you're sent to the Client Portal.
If not, it checks if the email belongs to an Employee. If that password matches, you're sent to the Admin Portal.
If neither matches, you get "Incorrect email or password" - on purpose, the app never says which part was wrong, so it can't be used to guess valid emails.

Once logged in, the app remembers you using a secure cookie, containing a "role" (Client, Admin, Designer, or Sales). Every page checks that role before deciding what to show.

Nobody can self-register as staff. Client accounts can be created two ways: someone registers themselves through the public site, or an Admin creates one manually. Staff accounts can only be created by an Admin - there's no public "sign up as staff" page, on purpose.

5. Who can see what (staff roles)
Feature	Admin	Sales	Designer
Dashboard	Everything	Everything	Only their own projects/files
Manage Clients	Yes	Yes	No
Add Employee	Yes	No	No
Manage/Add/Edit Projects	Yes	Yes	No (see "My Projects" instead)
My Projects + Update Status	Yes	No	Yes, only their own
Upload Design Files	Yes	No	Yes, only for their own projects
Manage Portfolio (publish to public site)	Yes	No	No
Manage Inquiries	Yes	Yes	No
Reports	Yes	No	No

This is enforced in two places at once, on purpose:

The sidebar only shows links a role is allowed to click.
Program.cs also blocks the page directly, so even typing the URL by hand doesn't get someone past a restriction they shouldn't have.
6. The Public Website, feature by feature
Home - shows projects the Admin has specifically marked "Feature on Home page."
Portfolio - shows all published projects, filterable by Residential/Commercial.
Portfolio Details - one project's photo, description, type, location, year.
Contact - anyone can submit an inquiry, even without an account. It gets saved with their name/email typed into the form (a "guest" inquiry), ready for a Sales staff member to follow up on.
7. The Client Portal, feature by feature
Dashboard - quick stats (total/in progress/completed projects) plus a short list of recent ones.
My Projects - every project belonging to that client.
Project Details - the cover photo, description, a status tracker (Inquiry → In Progress → Review → Completed), and a table of design files with View (opens in browser) and Download buttons.
Account Settings - update name/phone.
Change Password - requires the current password first.
8. The Admin Portal, feature by feature
Dashboard - business-wide stats and a combined "recent activity" feed (new clients, new projects, new files, new inquiries).
Manage Clients - search, view, edit, and see each client's projects.
Add Employee - create staff accounts with a temporary password.
Manage Projects - the full list, filterable by status; create new ones (linking a real Client + Employee); edit everything about one.
Upload Design Files - attach a file to a project.
Manage Portfolio - a photo-grid view specifically for deciding what goes public, with a one-click Publish/Unpublish button.
Manage Inquiries - see everything that's come in through Contact, assign it to a staff member, change its status, and (new) turn it into a real Client account with one click - it reuses the name/email/ phone already typed into the original inquiry.
Reports - pick a report type (Projects/Clients/Inquiries) and a date range; export as CSV (opens in Excel) or print/save as PDF.
Change Password - same idea as the Client one, for staff.
