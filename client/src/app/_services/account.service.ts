import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { User } from '../_models/user';
import { BehaviorSubject, map } from 'rxjs';
import { environment } from 'src/environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrl;
  private currentUserSource = new BehaviorSubject<User | null>(null); //initially null
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient) { }

  //account login 
  login(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/login', model).pipe(
      map((response: User) => {
        const user = response;
        if(user) {
          localStorage.setItem('user', JSON.stringify(user))
          this.setCurrentUser(user);
        }
      })
    )
  }

  //register account, then auto login
  register(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/register', model).pipe(
      map(user => {
        if(user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSource.next(user)
        }
      })
    )
  }

  //update behavior subject
  setCurrentUser(user: User) {
    this.currentUserSource.next(user);
  }

  //account logout
  logout() {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }
}
