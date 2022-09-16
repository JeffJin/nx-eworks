import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {RouterModule} from '@angular/router';
import {AdworksMaterialModule} from '@app/adworks-material.module';
import {AssetsComponent} from '@app/modules/dashboard/assets/assets.component';
import {AddToPlaylistComponent} from '@app/modules/dashboard/assets/add-to-playlist/add-to-playlist.component';
import {AudiosComponent} from '@app/modules/dashboard/assets/audios/audios.component';
import {ImageShellComponent} from '@app/modules/dashboard/assets/images/image-shell.component';
import {UploadComponent} from '@app/modules/dashboard/assets/upload/upload.component';
import {VideosShellComponent} from '@app/modules/dashboard/assets/videos/videos-shell.component';
import {EditImageComponent} from '@app/modules/dashboard/assets/images/edit-image/edit-image.component';
import {EditAudioComponent} from '@app/modules/dashboard/assets/audios/edit-audio/edit-audio.component';
import {EditVideoComponent} from '@app/modules/dashboard/assets/videos/edit-video/edit-video.component';
import {WidgetsModule} from '@app/components/widgets/widgets.module';
import {StoreModule} from '@ngrx/store';
import {assetReducer} from './states/assets.reducers';
import {EffectsModule} from '@ngrx/effects';
import {ImageEffects} from '@app/modules/dashboard/assets/effects/image.effects';
import {VideoEffects} from '@app/modules/dashboard/assets/effects/video.effects';
import {AssetEffects} from '@app/modules/dashboard/assets/effects/asset.effects';
import {VideoDetailsComponent} from '@app/modules/dashboard/assets/videos/video-details/video-details.component';
import {VideoListComponent} from '@app/modules/dashboard/assets/videos/video-list/video-list.component';
import {CategoriesComponent} from '@app/modules/dashboard/assets/categories/categories.component';
import {ImageListComponent} from '@app/modules/dashboard/assets/images/image-list/image-list.component';
import {ImageDetailsComponent} from '@app/modules/dashboard/assets/images/image-details/image-details.component';
import {ImageListShellComponent} from '@app/modules/dashboard/assets/images/image-list/image-list-shell.component';
import {VideoListShellComponent} from '@app/modules/dashboard/assets/videos/video-list/video-list-shell.component';

@NgModule({
  declarations: [
    AddToPlaylistComponent,
    AudiosComponent,
    EditAudioComponent,
    ImageShellComponent,
    ImageListShellComponent,
    ImageListComponent,
    ImageDetailsComponent,
    EditImageComponent,
    UploadComponent,
    EditVideoComponent,
    VideosShellComponent,
    VideoListShellComponent,
    AssetsComponent,
    VideoDetailsComponent,
    VideoListComponent,
    CategoriesComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    AdworksMaterialModule,
    WidgetsModule,
    StoreModule.forFeature('assets', assetReducer),
    EffectsModule.forFeature([
      ImageEffects,
      VideoEffects,
      AssetEffects,
    ]),
  ]
})
export class AssetsModule {
}
