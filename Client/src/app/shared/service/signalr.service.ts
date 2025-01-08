import { inject, Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environment';
import { AuthService } from './auth.service';
import { IceCandidate, Transaction } from '../model/Transaction';
import { Track } from './cloudflare.service';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection;
  authService = inject(AuthService);
  constructor() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.baseUrl}/hubs/cloudflare`, {
        accessTokenFactory: () =>
          this.authService.appContext()?.token || '',
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
      })
      .build();
  }
  public startConnection(): void {
    this.hubConnection.start()
      .then(() => console.log('Connection started'))
      .catch(err => console.log('Error while starting connection: ' + err));
  }

  public stopConnection(): void {
    this.hubConnection.stop();
  }
  
  public newSession(sessionId:string, tracks:Track[]): void {
    this.hubConnection.invoke('NewSession',sessionId, tracks)
      .catch(err => console.error(err));
  }
  
  public AddedToRoom(callback: (sessionId:string, tracks:Track[]) => void): void {
    this.hubConnection.on('AddedToRoom', callback);
  }

  public UsersConnected(callback: (emails:string[]) => void): void {
    this.hubConnection.on('UsersConnected', callback);
  }
  // Old
  // public newOfferAwaitingListener(callback: (transaction: Transaction) => void): void {
  //   this.hubConnection.on('NewOfferAwaiting', callback);
  // }

  // public AnswerResponseListener(callback: (transaction: Transaction) => void): void {
  //   this.hubConnection.on('AnswerResponse', callback);
  // }

  // public addQueueJoinedListener(callback: (emails: string[]) => void): void {
  //   this.hubConnection.on('QueueJoined', callback);
  // }

  // public receivedIceCandidateFromServerListener(callback: (iceCandidate: string) => void): void {
  //   this.hubConnection.on('ReceivedIceCandidateFromServer', callback);
  // }

  // public AnswererResponseListener(callback: (transaction: Transaction) => void): void {
  //   this.hubConnection.on('AnswererResponse', callback);
  // }

  // public joinQueue(): void {
  //   this.hubConnection.invoke('JoinQueue')
  //     .catch(err => console.error(err));
  // }

  // public AddIceCandidate(iceCandidate: IceCandidate): void {
  //   this.hubConnection.invoke('AddIceCandidate',iceCandidate )
  //     .catch(err => console.error(err));
  // }

  // public sendOffer(offer: string): void {
  //   this.hubConnection.invoke('SendOffer', offer)
  //     .catch(err => console.error(err));
  // }

  // public sendAnswer(transaction: Transaction): void {
  //   this.hubConnection.invoke('SendAnswer', JSON.stringify(transaction))
  //     .catch(err => console.error(err));
  // }
}
