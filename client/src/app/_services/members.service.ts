import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Member } from '../_models/member';
import { environment } from 'src/environments/environment.development';
import { map, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = []; //used for caching

  constructor(private http: HttpClient) { }

  //JWT token is append by interceptor prior to sending the request

  getMembers() {
    if(this.members.length > 0) return of(this.members); //cache data in an array
    return this.http.get<Member[]>(this.baseUrl + 'users').pipe(
      map(members => {
        this.members = members
        return members
      })
    )
  }

  getMember(username: string) {
    const member = this.members.find(x => x.userName === username); //check array cache
    if(member) return of(member);
    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = {...this.members[index], ...member} //update the array cache
      })
    )
  }

}
