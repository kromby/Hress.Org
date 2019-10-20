import React from 'react';
import './index.css';

import top from './top_small.png';
import bottom from './bottom_small.png';

class UserStatistics extends React.Component {
    constructor(props) {
        super(props);
        this.state = {}
    }

    render() {
        return (
            <table className="body" width="180px" cellspacing="0" cellpadding="0" border="0">
                <tr>
                    <td colspan="3">
                        <img src={top} alt="layout" />
                    </td>
                </tr>
                <tr>
                    <td className="MiddleLeft" />
                    <td valign="top" className="contentData">
                        <p>
                            {/* <asp:Label ID="lblCaption" runat="server" Font-Bold="true"></asp:Label> */}
                            Fyrirsögn
                        </p>
                        Listi
                        {/* <asp:DataList ID="dtlUsers" runat="server">
                            <ItemTemplate>
                                <asp:HyperLink ID="hlkUser" NavigateUrl='<%# "~/Gang/Single.aspx?id=" + Eval("User.Id") %>'
                                    Text='<%# Eval("User.Username") %>' runat="server" />
                                -
                                <asp:Label ID="lblCount" runat="server" Text='<%# Eval("Count") %>'></asp:Label><br />
                            </ItemTemplate>
                        </asp:DataList> */}
                    </td>
                    <td class="MiddleRight" />
                </tr>
                <tr>
                    <td colspan="3">
                    <img src={bottom} alt="layout" />
                    </td>
                </tr>
                <tr>
                    <td className="gap" />
                </tr>
            </table>
        )
    }
}

export default UserStatistics