import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {RouterModule} from '@angular/router';
import {AdworksMaterialModule} from '@app/adworks-material.module';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {CustomersComponent} from '@app/modules/dashboard/customers/customers.component';
import {DevicesComponent} from '@app/modules/dashboard/devices/devices.component';
import {GroupsComponent} from '@app/modules/dashboard/groups/groups.component';
import {LicensesComponent} from '@app/modules/dashboard/licenses/licenses.component';
import {LocationComponent} from '@app/modules/dashboard/location/location.component';
import {OverviewComponent} from '@app/modules/dashboard/overview/overview.component';
import {PlaylistComponent} from '@app/modules/dashboard/playlist/playlist.component';
import {ReportsComponent} from '@app/modules/dashboard/reports/reports.component';
import {SettingsComponent} from '@app/modules/dashboard/settings/settings.component';
import {SideMenuComponent} from '@app/modules/dashboard/side-menu/side-menu.component';
import {AssetsModule} from '@app/modules/dashboard/assets/assets.module';
import {OfflineDevicesComponent} from '@app/modules/dashboard/devices/offline-devices/offline-devices.component';
import {OnlineDevicesComponent} from '@app/modules/dashboard/devices/online-devices/online-devices.component';
import {AddLocationComponent} from '@app/modules/dashboard/location/add-location/add-location.component';
import {EditLocationComponent} from '@app/modules/dashboard/location/edit-location/edit-location.component';
import {CreatePlaylistComponent} from '@app/modules/dashboard/playlist/create-playlist/create-playlist.component';
import {EditPlaylistComponent} from '@app/modules/dashboard/playlist/edit-playlist/edit-playlist.component';
import {ListPlaylistComponent} from '@app/modules/dashboard/playlist/list-playlist/list-playlist.component';
import {EditCustomerComponent} from '@app/modules/dashboard/customers/edit-customer/edit-customer.component';
import {CreateCustomerComponent} from '@app/modules/dashboard/customers/create-customer/create-customer.component';
import {WidgetsModule} from '@app/components/widgets/widgets.module';
import {DashboardComponent} from '@app/modules/dashboard/dashboard.component';
import {DashboardRoutingModule} from '@app/modules/dashboard/dashboard-routing.module';
import {AccountComponent} from '@app/modules/dashboard/account/account.component';

@NgModule({
  declarations: [
    AccountComponent,
    CustomersComponent,
    EditCustomerComponent,
    CreateCustomerComponent,
    DevicesComponent,
    OfflineDevicesComponent,
    OnlineDevicesComponent,
    GroupsComponent,
    LicensesComponent,
    LocationComponent,
    AddLocationComponent,
    EditLocationComponent,
    OverviewComponent,
    PlaylistComponent,
    CreatePlaylistComponent,
    EditPlaylistComponent,
    ListPlaylistComponent,
    ReportsComponent,
    SettingsComponent,
    SideMenuComponent,
    DashboardComponent,
  ],
  imports: [
    CommonModule,
    RouterModule,
    AdworksMaterialModule,
    FormsModule,
    ReactiveFormsModule,
    WidgetsModule,
    AssetsModule,
    DashboardRoutingModule,
  ]
})
export class DashboardModule {

}
