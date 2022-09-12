import {
  Action,
  ActionReducer,
  ActionReducerMap,
  createReducer,
  MetaReducer,
  on} from '@ngrx/store';
import {AppState} from './app.state';
import {InjectionToken} from '@angular/core';
import {CacheService} from '../services/cache.service';
import {UserDto} from '../models/dtos';
import {authReducer} from '@app/states/auth/auth.reducers';
import {routerReducer} from '@ngrx/router-store';
import {logout} from '@app/states/auth/auth.actions';

export const ROOT_REDUCERS = new InjectionToken<ActionReducerMap<AppState, Action>>('Root reducers token', {
  factory: () => ({
    auth: authReducer,
    router: routerReducer,
  }),
});

export function metaReducerFactory(cacheService: CacheService): MetaReducer<never> {

  function restoreFromCache<S extends AppState, A extends Action = Action>(reducer: ActionReducer<S, A>): ActionReducer<S, A> {
      let onInit = true;
      return (state: S, action: A) => {
        let nextState = reducer(state, action);

        console.groupCollapsed(action.type);
        console.log('prev state', state);
        console.log('action', action);
        console.log('next state', nextState);

        if (onInit) {
          onInit = false;
          const user: UserDto = cacheService.getUser();
          const token = cacheService.getToken();
          if (nextState && user) {
            nextState = {...nextState, auth: {user, token}};
          }
          console.log('initialize state', nextState);
        }
        console.groupEnd();
        return nextState;
      };
  }

  return restoreFromCache;
}

export function logger(reducer: ActionReducer<AppState>): ActionReducer<AppState> {
  return (state, action) => {
    const result = reducer(state, action);
    console.groupCollapsed(action.type);
    console.log('prev state', state);
    console.log('action', action);
    console.log('next state', result);
    console.groupEnd();

    return result;
  };
}
