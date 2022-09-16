import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {DashboardComponent} from '@app/modules/dashboard/dashboard.component';
import {CustomersComponent} from '@app/modules/dashboard/customers/customers.component';
import {DevicesComponent} from '@app/modules/dashboard/devices/devices.component';
import {GroupsComponent} from '@app/modules/dashboard/groups/groups.component';
import {LicensesComponent} from '@app/modules/dashboard/licenses/licenses.component';
import {LocationComponent} from '@app/modules/dashboard/location/location.component';
import {OverviewComponent} from '@app/modules/dashboard/overview/overview.component';
import {PlaylistComponent} from '@app/modules/dashboard/playlist/playlist.component';
import {ReportsComponent} from '@app/modules/dashboard/reports/reports.component';
import {SettingsComponent} from '@app/modules/dashboard/settings/settings.component';
import {OfflineDevicesComponent} from '@app/modules/dashboard/devices/offline-devices/offline-devices.component';
import {OnlineDevicesComponent} from '@app/modules/dashboard/devices/online-devices/online-devices.component';
import {CreatePlaylistComponent} from '@app/modules/dashboard/playlist/create-playlist/create-playlist.component';
import {EditPlaylistComponent} from '@app/modules/dashboard/playlist/edit-playlist/edit-playlist.component';
import {ListPlaylistComponent} from '@app/modules/dashboard/playlist/list-playlist/list-playlist.component';
import {PrivateGuard} from '@app/guards/private.guard';
import {UploadComponent} from '@app/modules/dashboard/assets/upload/upload.component';
import {AssetsComponent} from '@app/modules/dashboard/assets/assets.component';
import {VideosShellComponent} from '@app/modules/dashboard/assets/videos/videos-shell.component';
import {ImageShellComponent} from '@app/modules/dashboard/assets/images/image-shell.component';
import {AudiosComponent} from '@app/modules/dashboard/assets/audios/audios.component';
import {VideoDetailsComponent} from '@app/modules/dashboard/assets/videos/video-details/video-details.component';
import {ImageDetailsComponent} from '@app/modules/dashboard/assets/images/image-details/image-details.component';
import {AccountComponent} from '@app/modules/dashboard/account/account.component';
import {EditImageComponent} from '@app/modules/dashboard/assets/images/edit-image/edit-image.component';
import {ImageListShellComponent} from '@app/modules/dashboard/assets/images/image-list/image-list-shell.component';
import {VideoListShellComponent} from '@app/modules/dashboard/assets/videos/video-list/video-list-shell.component';
import {EditVideoComponent} from '@app/modules/dashboard/assets/videos/edit-video/edit-video.component';

export const dashboardRoutes: Routes = [
  { path: '', component: DashboardComponent,
    children: [
      { path: '', redirectTo: 'overview', pathMatch: 'full' },
      { path: 'overview', component: OverviewComponent },
      { path: 'groups', component: GroupsComponent },
      { path: 'account', component: AccountComponent },
      { path: 'assets', component: AssetsComponent,
        children: [
          { path: '', redirectTo: 'videos', pathMatch: 'full' },
          { path: 'videos', component: VideosShellComponent,
            children: [
              { path: '', redirectTo: 'list', pathMatch: 'full' },
              { path: 'details', component: VideoDetailsComponent },
              { path: 'list', component: VideoListShellComponent },
            ]
          },
          { path: 'images', component: ImageShellComponent,
            children: [
              { path: '', redirectTo: 'list', pathMatch: 'full' },
              { path: 'list', component: ImageListShellComponent },
              { path: 'details', component: ImageDetailsComponent },
            ]
          },
          { path: 'audios', component: AudiosComponent },
          { path: 'upload', component: UploadComponent },
        ]
      },
      { path: 'devices', component: DevicesComponent,
        children: [
          { path: '', component: DevicesComponent },
          { path: 'offline', component: OfflineDevicesComponent },
          { path: 'online', component: OnlineDevicesComponent },
        ]
      },
      { path: 'reports', component: ReportsComponent },
      { path: 'playlists', component: PlaylistComponent,
        children: [
          { path: '', redirectTo: 'list', pathMatch: 'full' },
          { path: 'list', component: ListPlaylistComponent },
          { path: 'create', component: CreatePlaylistComponent },
          { path: 'edit/:id', component: EditPlaylistComponent }
        ]
      },
      { path: 'licenses', component: LicensesComponent },
      { path: 'locations', component: LocationComponent },
      { path: 'customers', component: CustomersComponent },
      { path: 'settings', component: SettingsComponent },
    ], canActivate: [PrivateGuard]
  }
];


@NgModule({
  imports: [
    RouterModule.forChild(dashboardRoutes),
  ],
  exports: [RouterModule],
})
export class DashboardRoutingModule {

}
