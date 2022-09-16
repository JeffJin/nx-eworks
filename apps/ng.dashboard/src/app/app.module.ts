import {BrowserModule} from '@angular/platform-browser';
import {APP_INITIALIZER, CUSTOM_ELEMENTS_SCHEMA, ErrorHandler, NgModule} from '@angular/core';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {HTTP_INTERCEPTORS, HttpClientModule} from '@angular/common/http';
import {META_REDUCERS, MetaReducer, StoreModule} from '@ngrx/store';
import {EffectsModule} from '@ngrx/effects';
import {StoreDevtoolsModule} from '@ngrx/store-devtools';

import {AppComponent} from './app.component';
import {PageNotFoundComponent} from './components/page-not-found/page-not-found.component';
import {LoginComponent} from './components/login/login.component';
import {ChannelService, SignalrWindow} from './services/channel.service';
import {AuthService} from './services/auth.service';
import {UserService} from '@app/services/user.service';
import {TokenInterceptor} from './services/token.interceptor';
import {CacheService} from './services/cache.service';
import {FullScreenVideoComponent} from './components/full-screen-video/full-screen-video.component';
import {WindowRef} from './services/window-ref';
import {VideoService} from './services/video.service';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {UiService} from './services/ui.service';
import {PrivateGuard} from './guards/private.guard';
import {NotAllowedComponent} from './components/error/not-allowed/not-allowed.component';
import {AdminGuard} from './guards/admin.guard';
import {ErrorComponent} from './components/error/error.component';
import {ServerErrorComponent} from './components/error/server-error/server-error.component';
import {Utils} from './services/utils';
import {ConfirmEmailComponent} from './components/confirm-email/confirm-email.component';
import {FileService} from './services/file.service';
import {ImageService} from './services/image.service';
import {AudioService} from './services/audio.service';
import {MinuteSecondsPipe} from './pipes/minute-seconds.pipe';
import {DeviceService} from './services/device.service';
import {GroupService} from './services/group.service';
import {CustomerService} from './services/customer.service';
import {PlaylistService} from './services/playlist.service';
import {LocationService} from './services/location.service';
import {PlaylistPlayerComponent} from './components/playlist-player/playlist-player.component';
import {ProgressComponent} from './components/progress/progress.component';
import {SignalrService} from './services/signalr.service';
import {MessageQueueService} from './services/message-queue.service';
import {AdworksMaterialModule} from './adworks-material.module';
import {AppRoutingModule} from './app-routing.module';
import {CustomErrorHandler} from './handlers/custom-error-handler';
import {metaReducerFactory, ROOT_REDUCERS} from './states/app.reducer';
import {NavbarComponent} from './components/shared/navbar/navbar.component';
import {FooterComponent} from './components/shared/footer/footer.component';
import {AuthFooterComponent} from './components/shared/auth-footer/auth-footer.component';
import {LiveStreamComponent} from './components/live-stream/live-stream.component';
import {PromotionComponent} from './components/promotion/promotion.component';
import {RegisterComponent} from './components/register/register.component';
import {appInitializer} from './services/app.initializer';
import {environment} from '@environments/environment';
import {DialogsModule} from '@app/components/dialogs/dialogs.module';
import {WidgetsModule} from '@app/components/widgets/widgets.module';
import {CommonService} from '@app/services/common.service';
import {AuthEffects} from '@app/states/auth/effects/auth.effects';
import {RouteReuseStrategy} from '@angular/router';
import {CustomRouteReuseStrategy} from '@app/custom-route-reuse-strategy';
import { ServiceWorkerModule } from '@angular/service-worker';

@NgModule({
  declarations: [
    AppComponent,
    NavbarComponent,
    FooterComponent,
    AuthFooterComponent,
    LoginComponent,
    RegisterComponent,
    PageNotFoundComponent,
    FullScreenVideoComponent,
    NotAllowedComponent,
    ErrorComponent,
    ServerErrorComponent,
    ConfirmEmailComponent,
    MinuteSecondsPipe,
    PlaylistPlayerComponent,
    ProgressComponent,
    LiveStreamComponent,
    PromotionComponent,
  ],
  imports: [
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    BrowserModule,
    AdworksMaterialModule,
    BrowserAnimationsModule,
    AppRoutingModule,
    StoreModule.forRoot(ROOT_REDUCERS, {
      runtimeChecks: {
        strictStateSerializability: true,
        strictActionSerializability: true,
        strictActionWithinNgZone: true,
        strictActionTypeUniqueness: true,
      },
    }),
    EffectsModule.forRoot([AuthEffects]),
    StoreDevtoolsModule.instrument({
      maxAge: 25, // Retains last 25 states
      logOnly: environment.production, // Restrict extension to log-only mode
      autoPause: true, // Pauses recording actions and state changes when the extension window is not open
    }),
    DialogsModule,
    WidgetsModule,
    ServiceWorkerModule.register('ngsw-worker.js', {
      enabled: environment.production,
      // Register the ServiceWorker as soon as the application is stable
      // or after 30 seconds (whichever comes first).
      registrationStrategy: 'registerWhenStable:10000'
    }),
  ],
  providers: [
    {provide: APP_INITIALIZER, useFactory: appInitializer, multi: true, deps: [AuthService]},
    AdminGuard,
    PrivateGuard,
    UiService,
    AuthService,
    UserService,
    CacheService,
    ChannelService,
    CommonService,
    VideoService,
    ImageService,
    AudioService,
    FileService,
    DeviceService,
    GroupService,
    CustomerService,
    PlaylistService,
    LocationService,
    Utils,
    WindowRef,
    SignalrService,
    MessageQueueService,
    {provide: SignalrWindow, useValue: window},
    {provide: ErrorHandler, useClass: CustomErrorHandler},
    {
      provide: HTTP_INTERCEPTORS,
      useClass: TokenInterceptor,
      multi: true
    },
    {
      provide: META_REDUCERS,
      deps: [CacheService],
      useFactory: metaReducerFactory,
      multi: true
    },
    {
      provide: RouteReuseStrategy,
      useClass: CustomRouteReuseStrategy,
    }
  ],
  bootstrap: [AppComponent],
  exports: [

  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class AppModule {
}
