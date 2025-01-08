import { Component, ElementRef, inject, ViewChild } from '@angular/core';
import { CloudflareService, Track, TracksRequestTrack, TracksResponse } from '../../shared/service/cloudflare.service';
import { SignalRService } from '../../shared/service/signalr.service';
import { CommonModule } from '@angular/common';
export interface Connection {
  sessionId: string;
  peerConnection: RTCPeerConnection;
}
@Component({
    selector: 'app-meet',
    imports: [CommonModule],
    templateUrl: './meet.component.html',
    styleUrl: './meet.component.scss'
})
export class MeetComponent {
  @ViewChild('localVideo', { static: true, read: ElementRef }) localVideoElement!: ElementRef;
  @ViewChild('remoteVideo', { static: true, read: ElementRef }) remoteVideoElement!: ElementRef;
  @ViewChild('matchBtn', { static: true, read: ElementRef }) matchBtn!: ElementRef;
  cloudflareService = inject(CloudflareService);
  signalRService = inject(SignalRService);
  localSession!: Connection;
  remoteSession: Connection | undefined;
  localTracks: TracksRequestTrack[] = [];
  remoteTracks: Track[] = [];
  emails: string[] = [];
  pushedlocalTracksResponse!: TracksResponse

  showSidebar = false;
  paused = false;
  async ngOnInit(): Promise<void> {
    this.signalRService.startConnection();

    //#region signalr listeners
    this.signalRService.AddedToRoom(async (sessionId: string, tracks: Track[]) => {
      await this.answer(sessionId, tracks);
    })

    this.signalRService.UsersConnected(async (emails: string[]) => {
      console.log(emails)
      this.emails = emails;
    })
    //#endregion

    // get a MediaStream from the user's webcam & mic
    const media = await navigator.mediaDevices.getUserMedia({
      audio: true,
      video: true,
    });

    // show the local feed
    this.localVideoElement.nativeElement.srcObject = media;

    // First, we'll establish the "local" Calls session by calling createCallsSession
    // which is defined towards the bottom of this script. This will create an
    // RTCPeerConnection and a Calls session, and connect the two.
    this.localSession = await this.createCallsSession();
    //local tracks
    const transceivers = media.getTracks().map((track) =>
      this.localSession.peerConnection.addTransceiver(track, {
        direction: "sendonly",
      }),
    );

    // Now that the peer connection has tracks, the next step is to create and set a
    // new offer as the local description. This offer will contain the new tracks in
    // its session description.
    await this.localSession.peerConnection.setLocalDescription(
      await this.localSession.peerConnection.createOffer(),
    );
    this.localTracks = transceivers.map(({ mid, sender }) => ({
      location: "local",
      mid,
      trackName: sender.track?.id,
    }));
    // Send the local session description to the Calls API, it will
    // respond with an answer and trackIds.
    this.pushedlocalTracksResponse = await this.cloudflareService.newTracks(this.localSession.sessionId, this.localTracks, this.localSession.peerConnection.localDescription?.sdp);

    // We take the answer we got from the Calls API and set it as the
    // peer connection's remote description.
    await this.localSession.peerConnection.setRemoteDescription(new RTCSessionDescription(this.pushedlocalTracksResponse.sessionDescription));
    this.matchBtn.nativeElement.disabled = false;
  }

  async match() {
    //close tracks if any
    if (this.remoteSession?.sessionId && this.remoteTracks.length > 0 && this.remoteSession.peerConnection.remoteDescription != null) {
      console.log(this.remoteSession.sessionId, this.remoteTracks.map((track: Track) => track.mid), this.remoteSession.peerConnection.remoteDescription.sdp)
      this.cloudflareService.closeTracks(this.remoteSession.sessionId, this.remoteTracks.map((track: Track) => track.mid), this.remoteSession.peerConnection.remoteDescription.sdp, false)
    }
    console.log("Match start")
    //send signaling 
    this.signalRService.newSession(this.localSession.sessionId, this.pushedlocalTracksResponse.tracks);
  }

  async answer(sessionId: string, tracks: Track[]) {
    // 🌀🌀🌀
    // At this point, we're done with the sending "local" side, and
    // can now pretend that we're in a completely different browser
    // tab to receive on the "remote" side, and have received the
    // session id and track information to pull via some signalling
    // method such as WebSockets.
    const tracksToPull = tracks.map((sender) => ({
      location: "remote",
      trackName: sender.trackName,
      sessionId: sessionId,
    }));

    // Let's create the remoteSession now to pull the tracks
    this.remoteSession = await this.createCallsSession();
    if (this.remoteSession) {
      // We're going to modify the remote session and pull these tracks
      // by requesting an offer from the Calls API with the tracks we
      // want to pull. Play the remote tracks in this session.
      const pullResponse = await this.cloudflareService.newTracks(this.remoteSession.sessionId, tracksToPull);
      this.remoteTracks = pullResponse.tracks;
      // We set up this promise before updating local and remote descriptions
      // so the "track" event listeners are already in place before they fire.
      const resolvingTracks = Promise.all(
        pullResponse.tracks.map(
          ({ mid }) =>
            // This will resolve when the track for the corresponding mid is added.
            new Promise<MediaStreamTrack>((res, rej) => {
              setTimeout(rej, 5000);
              const handleTrack = ({ transceiver, track }: { transceiver: RTCRtpTransceiver; track: MediaStreamTrack }) => {
                if (transceiver.mid !== mid) return;
                this.remoteSession?.peerConnection.removeEventListener(
                  "track",
                  handleTrack,
                );
                res(track);
              };
              this.remoteSession?.peerConnection.addEventListener(
                "track",
                handleTrack,
              );
            }),
        ),
      );

      // Handle renegotiation, this will always be true when pulling tracks
      if (pullResponse.requiresImmediateRenegotiation) {
        // We got a session description from the remote in the response,
        // we need to set it as the remote description
        this.remoteSession.peerConnection.setRemoteDescription(pullResponse.sessionDescription);
        // Create and set the answer as local description
        await this.remoteSession.peerConnection.setLocalDescription(await this.remoteSession.peerConnection.createAnswer());
        // Send our answer back to the Calls API
        const renegotiateResponse = this.cloudflareService.sendAnswerSDP(this.remoteSession.peerConnection.currentLocalDescription?.sdp!, this.remoteSession.sessionId)
        // if (renegotiateResponse?.errorCode) {
        //   throw new Error(renegotiateResponse.errorDescription);
        // }
      }

      // Now we wait for the tracks to resolve
      const pulledTracks = await resolvingTracks;
      // Lastly, we set them in the remoteVideo to display
      const remoteVideoStream = new MediaStream();
      this.remoteVideoElement.nativeElement.srcObject = remoteVideoStream;
      pulledTracks.forEach((t: MediaStreamTrack) => remoteVideoStream.addTrack(t));
    }

  }

  async closeRemoteTracks(sessionId: string, tracks: string[], sdp?: string, force?: boolean) {
    await this.cloudflareService.closeTracks(sessionId, tracks, sdp, force);
  }
  async createCallsSession() {
    const peerConnection = new RTCPeerConnection({
      iceServers: [
        {
          urls: "stun:stun.cloudflare.com:3478",
        },
      ],
      bundlePolicy: "max-bundle",
    });

    // in order for the ICE connection to be established, there must
    // be at least one track present, but since we want each peer
    // connection and session to have tracks explicitly pushed and
    // pulled, we can add an empty audio track here to force the
    // connection to be established.
    peerConnection.addTransceiver("audio", {
      direction: "inactive",
    });

    // create an offer and set it as the local description
    await peerConnection.setLocalDescription(
      await peerConnection.createOffer(),
    );
    const { sessionId, sessionDescription } = await this.cloudflareService.newSession(peerConnection.localDescription?.sdp!);
    const connected = new Promise((res, rej) => {
      // timeout after 5s
      setTimeout(rej, 5000);
      const iceConnectionStateChangeHandler = () => {
        if (peerConnection.iceConnectionState === "connected") {
          peerConnection.removeEventListener(
            "iceconnectionstatechange",
            iceConnectionStateChangeHandler,
          );
          res(undefined);
        }
      };
      peerConnection.addEventListener(
        "iceconnectionstatechange",
        iceConnectionStateChangeHandler,
      );
    });

    // Once both local and remote descriptions are set, the ICE process begins
    await peerConnection.setRemoteDescription(sessionDescription);
    // Wait until the peer connection's iceConnectionState is "connected"
    await connected;
    return {
      peerConnection,
      sessionId,
    };
  }
  toggleSidebar() {
    this.showSidebar = !this.showSidebar;
  }
  pause() {
    this.paused = !this.paused;
    this.remoteVideoElement.nativeElement.pause();
    this.localVideoElement.nativeElement.pause();
  }
  play() {
    this.paused = !this.paused;
    this.remoteVideoElement.nativeElement.play();
    this.localVideoElement.nativeElement.play();
  }
}
