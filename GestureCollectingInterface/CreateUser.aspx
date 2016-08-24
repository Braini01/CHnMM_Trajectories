<%@ Page Title="About Us" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="CreateUser.aspx.cs" Inherits="GestureCollectingInterface.CreateUser" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>Create New User</h2>

    <asp:Table ID="layoutTable" runat="server">
    <asp:TableRow>
        <asp:TableCell><asp:Label ID="lblUsername" runat="server" Text="Username:"></asp:Label></asp:TableCell>
        <asp:TableCell><asp:TextBox ID="tbUsername" runat="server"></asp:TextBox></asp:TableCell>
    </asp:TableRow>
    <asp:TableRow>
        <asp:TableCell><asp:Label ID="lblName" runat="server" Text="Name:"></asp:Label></asp:TableCell>
        <asp:TableCell><asp:TextBox ID="tbName" runat="server" Wrap="False"></asp:TextBox></asp:TableCell>
    </asp:TableRow>
    </asp:Table>

    <asp:Button ID="btnCreateUser" runat="server" Text="Create User" 
        onclick="btnCreateUser_Click" />
</asp:Content>
