// import { inject, Injectable } from '@angular/core';
// import { SignalRService } from './signalr.service';
// import { IceCandidate, Transaction } from '../model/Transaction';
// import { AuthService } from './auth.service';

// @Injectable({
//   providedIn: 'root'
// })
// export class WebRTCService {
//   private peerConnection: RTCPeerConnection;
//   signalRService = inject(SignalRService);
//   authService = inject(AuthService);
//   isMyOffer = false;
//   constructor() {
//     this.peerConnection = new RTCPeerConnection({
//       iceServers: [{
//         urls: ['stun:stun.l.google.com:19302', 'stun:stun1.l.google.com:19302', 'stun:stun2.l.google.com:19302', 'stun:stun3.l.google.com:19302', 'stun:stun4.l.google.com:19302']
//       }]
//     });
//     this.peerConnection.addEventListener('icecandidate', e => {
//       if (e.candidate) {
//         this.signalRService.AddIceCandidate({
//           candiate: JSON.stringify(e.candidate.toJSON()),
//           email: this.authService.appContext()!.user.email,
//           isMyOffer: this.isMyOffer,
//         })
//       }
//     })
//   }

//   public async startLocalStream(localStream: MediaStream): Promise<MediaStream> {
//     localStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
//     localStream.getTracks().forEach(track => {
//       this.peerConnection.addTrack(track, localStream);
//     })
//     // this.peerConnection.addEventListener('signalingstatechange', (e) => {
//     //   console.log(e);
//     //   console.log(this.peerConnection.signalingState);
//     // })
//     return localStream;
//   }

//   public addRemoteStreamListener(remoteStream: MediaStream): void {
//     //get track from the other end.
//     this.peerConnection.ontrack = (event) => {
//       console.log("Incoming remote track...");
//       event.streams[0].getTracks().forEach(track => {
//         remoteStream.addTrack(track);
//       })
//     };
//   }
//   public async sendOffer(): Promise<any> {
//     this.isMyOffer = true;
//     // Create offer and set local description
//     const offer = await this.peerConnection.createOffer();
//     await this.peerConnection.setLocalDescription(offer);
//     this.signalRService.sendOffer(JSON.stringify(offer));
//   }

//   public async createAnswer(transaction: Transaction): Promise<RTCSessionDescriptionInit> {
//     // Set remote description and create answer
//     await this.peerConnection.setRemoteDescription(JSON.parse(transaction.offer));
//     const answer = await this.peerConnection.createAnswer();
//     await this.peerConnection.setLocalDescription(answer);
//     return answer;
//   }

//   public addNewIceCandiate(iceCandidate:string){
//     this.peerConnection.addIceCandidate(JSON.parse(iceCandidate))
//   }
// }
