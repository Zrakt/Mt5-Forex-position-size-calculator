
// #include "Cassette.h"
// #include "CassetteApp.h"
// #include "Dialog.h"

// #ifdef _DEBUG
// #define new DEBUG_NEW
// #undef THIS_FILE
// static char THIS_FILE[] = __FILE__;
// #endif

// /////////////////////////////////////////////////////////////////////////////
// // Dialog dialog

// Dialog::Dialog( UINT nIDTemplate, CWnd * pParentWnd ) : CDialog( nIDTemplate, pParentWnd ), m_nIDTemplate(nIDTemplate)
// {
//     //{{AFX_DATA_INIT(Dialog)
//         // NOTE: the ClassWizard will add member initialization here
//     //}}AFX_DATA_INIT
// }

// BEGIN_MESSAGE_MAP(Dialog, CDialog)
//     //{{AFX_MSG_MAP(Dialog)
//     //}}AFX_MSG_MAP

//     //ON_WM_DRAWITEM()
//     //ON_WM_CTLCOLOR()
//     //ON_WM_ERASEBKGND()
//     ON_NOTIFY_EX( TTN_NEEDTEXT, 0, OnToolTipText )
//     ON_WM_INITMENUPOPUP()
// END_MESSAGE_MAP()

// /////////////////////////////////////////////////////////////////////////////
// // Dialog message handlers
// BOOL Dialog::OnToolTipText( UINT id, NMHDR * pNMHDR, LRESULT * pResult )
// {
//     TOOLTIPTEXT * pTTT = (TOOLTIPTEXT *)pNMHDR;
//     UINT nID = 0;
//     if ( pTTT->uFlags & TTF_IDISHWND )
//     {
//         // idFrom is actually the HWND of the tool
//         nID = ::GetDlgCtrlID( reinterpret_cast<HWND>(pNMHDR->idFrom) );
//         if ( nID != 0 )
//         {
//             pTTT->lpszText = MAKEINTRESOURCE(nID); // ����ͬID���ַ�����Դ����ʾ�ı�
//             pTTT->hinst = AfxGetResourceHandle();
//             return TRUE;
//         }
//     }
//     return FALSE;
    
// }

// BOOL Dialog::PreTranslateMessage( MSG * pMsg ) 
// {
//     // ��ToolTips Ctrl����ϵͳ������ToolTips�����Ϣ
//     if ( m_ToolTips )
//     {
//         m_ToolTips.RelayEvent(pMsg);
//     }
    
//     return CDialog::PreTranslateMessage(pMsg);
// }

// BOOL Dialog::OnInitDialog()
// {
//     BOOL bRet = CDialog::OnInitDialog();

//     //this->EnableToolTips(TRUE); // ���ô����Դ���ToolTips����,�ù��ܴ�����ʾ��λ��bug

//     m_ToolTips.Create( this, TTS_BALLOON | TTS_ALWAYSTIP ); // ������ʾ���

//     //m_ToolTips.SetTipTextColor( RGB( 255, 96, 0 ) ); // ������ʾ�ı���ɫ

//     // ö���Ӵ��ڣ�����������ToolTips������ӵ�����Ի���
//     CWnd * pChildWnd;
//     if ( pChildWnd = this->GetWindow(GW_CHILD) ) do
//     {
//         m_ToolTips.AddTool(pChildWnd);
//         //CString clsName;
//         //int nChars = GetClassName( pChildWnd->GetSafeHwnd(), clsName.GetBuffer(256), 256 );
//         //clsName.GetBufferSetLength(nChars);
//         //if ( !clsName.IsEmpty() ) clsName.MakeUpper();
//         //if ( clsName == _T("STATIC") )
//         //{
//         //    size_t i = m_allLabels.size();
//         //    m_allLabels.push_back( winplus::MakeSimple( new Label() ) );
//         //    m_allLabels[i]->SubclassWindow( pChildWnd->GetSafeHwnd() );
//         //    m_allLabels[i]->SetColor( RGB( 255, 2, 3 ) );

//         //    pChildWnd->UpdateWindow();
//         //}
//     }
//     while ( pChildWnd = pChildWnd->GetWindow(GW_HWNDNEXT) );

//     m_ToolTips.Activate(TRUE);

//     return bRet;
// }

// void Dialog::OnInitMenuPopup( CMenu * pMenu, UINT nIndex, BOOL bSysMenu )
// {
//     if (bSysMenu)
//         return;     // don't support system menu
    
//     ASSERT(pMenu != NULL);
//     // check the enabled state of various menu items
    
//     CCmdUI state;
//     state.m_pMenu = pMenu;
//     ASSERT(state.m_pOther == NULL);
//     ASSERT(state.m_pParentMenu == NULL);
    
//     // determine if menu is popup in top-level menu and set m_pOther to
//     //  it if so (m_pParentMenu == NULL indicates that it is secondary popup)
//     HMENU hParentMenu;
//     if (AfxGetThreadState()->m_hTrackingMenu == pMenu->m_hMenu)
//         state.m_pParentMenu = pMenu;    // parent == child for tracking popup
//     else if ((hParentMenu = ::GetMenu(m_hWnd)) != NULL)
//     {
//         CWnd* pParent = GetTopLevelParent();
//         // child windows don't have menus -- need to go to the top!
//         if (pParent != NULL &&
//             (hParentMenu = ::GetMenu(pParent->m_hWnd)) != NULL)
//         {
//             int nIndexMax = ::GetMenuItemCount(hParentMenu);
//             for (int nIndex = 0; nIndex < nIndexMax; nIndex++)
//             {
//                 if (::GetSubMenu(hParentMenu, nIndex) == pMenu->m_hMenu)
//                 {
//                     // when popup is found, m_pParentMenu is containing menu
//                     state.m_pParentMenu = CMenu::FromHandle(hParentMenu);
//                     break;
//                 }
//             }
//         }
//     }
    
//     state.m_nIndexMax = pMenu->GetMenuItemCount();
//     for (state.m_nIndex = 0; state.m_nIndex < state.m_nIndexMax; state.m_nIndex++)
//     {
//         state.m_nID = pMenu->GetMenuItemID(state.m_nIndex);
//         if (state.m_nID == 0)
//             continue; // menu separator or invalid cmd - ignore it
        
//         ASSERT(state.m_pOther == NULL);
//         ASSERT(state.m_pMenu != NULL);
//         if (state.m_nID == (UINT)-1)
//         {
//             // possibly a popup menu, route to first item of that popup
//             state.m_pSubMenu = pMenu->GetSubMenu(state.m_nIndex);
//             if (state.m_pSubMenu == NULL ||
//                 (state.m_nID = state.m_pSubMenu->GetMenuItemID(0)) == 0 ||
//                 state.m_nID == (UINT)-1)
//             {
//                 continue;       // first item of popup can't be routed to
//             }
//             state.DoUpdate(this, FALSE);    // popups are never auto disabled
//         }
//         else
//         {
//             // normal menu item
//             // Auto enable/disable if frame window has 'm_bAutoMenuEnable'
//             //    set and command is _not_ a system command.
//             state.m_pSubMenu = NULL;
//             state.DoUpdate(this, TRUE && state.m_nID < 0xF000);
//         }

//         // adjust for menu deletions and additions
//         UINT nCount = pMenu->GetMenuItemCount();
//         if (nCount < state.m_nIndexMax)
//         {
//             state.m_nIndex -= (state.m_nIndexMax - nCount);
//             while (
//                 state.m_nIndex < nCount &&
//                 pMenu->GetMenuItemID(state.m_nIndex) == state.m_nID
//             )
//             {
//                 state.m_nIndex++;
//             }
//         }
//         state.m_nIndexMax = nCount;
//     }
// }

// void Dialog::OnDrawItem(int nIDCtl, LPDRAWITEMSTRUCT lpDrawItemStruct) 
// {

//     CDialog::OnDrawItem(nIDCtl, lpDrawItemStruct);
// }

// BOOL Dialog::OnEraseBkgnd( CDC * pDC )
// {
//     using namespace Gdiplus;
//     Gdiplus::Graphics g( pDC->GetSafeHdc() );
//     Rect clientRect = winplus::RectGdiToGdiplus<Rect>( winplus::Window_GetClient( GetSafeHwnd() ) );

//     LinearGradientBrush brush(
//         clientRect,
//         Color( 0xe3,0xbf,0xcd),
//         /*Color( 255,255,255 )*/winplus::COLORREF_to_Color(0x7c3d56),
//         LinearGradientModeForwardDiagonal
//     );
//     g.FillRectangle( &brush, clientRect );//*/

//     return 1;//CDialog::OnEraseBkgnd(pDC);
// }

// HBRUSH Dialog::OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor) 
// {
//     HBRUSH hbr;
//     hbr = CDialog::OnCtlColor(pDC, pWnd, nCtlColor);

//     switch ( nCtlColor )
//     {
//     case CTLCOLOR_STATIC:
//         {
//             CString clsName;
//             GetClassName( pWnd->GetSafeHwnd(), clsName.GetBuffer(256), 256 );
//             clsName.MakeUpper();

//             DWORD dwStyle = pWnd->GetStyle();

//             if ( (dwStyle & BS_GROUPBOX) == BS_GROUPBOX ) // Static���
//             {
//                 pDC->SetBkMode(TRANSPARENT);
//                 pDC->SetTextColor( RGB(255,255,255) );
//                 //hbr = (HBRUSH)GetStockObject(NULL_BRUSH);
//             }
//             else if ( (dwStyle & BS_AUTOCHECKBOX) == BS_AUTOCHECKBOX ) // checkbox
//             {
//                 pDC->SetBkMode(TRANSPARENT);
//                 //hbr = (HBRUSH)GetStockObject(NULL_BRUSH);
//             }
//             else if ( clsName == "SYSLINK" )
//             {
//                 //LOG( window_get_text(*pWnd) );
//                 pDC->SetBkMode(TRANSPARENT);
//                 hbr = (HBRUSH)GetStockObject(NULL_BRUSH);
//             }
//             else if ( clsName == "STATIC" && (dwStyle & SS_ICON) == SS_ICON )
//             {
//                 hbr = (HBRUSH)GetStockObject(NULL_BRUSH);
//             }
//             else if ( clsName == "STATIC" ) // ��ǩ
//             {
//                 pDC->SetBkMode(TRANSPARENT);
//                 hbr = (HBRUSH)GetStockObject(NULL_BRUSH);
//             }
//         }
//         break;
//     case CTLCOLOR_BTN:
//         hbr = (HBRUSH)GetStockObject(NULL_BRUSH);
//         break;
//     }

//     return hbr;
// }
