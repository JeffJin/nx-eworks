import {environment} from '../../environments/environment';
import {AuthService} from './auth.service';

export const appInitializer = (authService: AuthService) => {
    return () => new Promise((resolve, reject) => {
      // @ts-ignore
      window.fbAsyncInit = () => {
        FB.init({
          appId      : environment.facebookAppId,
          xfbml      : true,
          version    : 'v10.0'
        });
        FB.AppEvents.logPageView();

        FB.getLoginStatus(({authResponse}) => {   // Called after the JS SDK has been initialized.
          console.log('FB login status', authResponse);        // Returns the login status.

          if (authResponse) {
            FB.api('/me', (response) => {
              resolve(response);
            });
          } else {
            resolve(null);
          }
        });
      };

      // tslint:disable-next-line:typedef only-arrow-functions
      (function(d, s, id){
        // tslint:disable-next-line:prefer-const
        let js;
        const fjs = d.getElementsByTagName(s)[0];
        if (d.getElementById(id)) {return; }
        js = d.createElement(s); js.id = id;
        js.src = 'https://connect.facebook.net/en_US/sdk.js';
        fjs.parentNode.insertBefore(js, fjs);
      }(document, 'script', 'facebook-jssdk'));
    });
};
