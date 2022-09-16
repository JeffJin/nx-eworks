import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { RouterModule } from '@angular/router';
import { authRoutes, SecurityModule } from '@eworks/ng.security';

@NgModule({
  declarations: [AppComponent],
  imports: [
    BrowserModule,
    RouterModule.forRoot([{ path: 'auth', children: authRoutes }], { initialNavigation: "enabledNonBlocking" }),
    SecurityModule     // added
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {}
