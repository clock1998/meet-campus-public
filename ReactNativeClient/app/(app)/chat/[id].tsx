import { useSession } from '@/context/AuthContext';
import { CreateMessageResponse, CreateMessageRequest } from '@/services/signalRService';
import React, { useEffect, useState, useRef } from 'react';
import { View, Text, StyleSheet, TextInput, TouchableOpacity, KeyboardAvoidingView, Platform, ActivityIndicator } from 'react-native';
import { useLocalSearchParams, useNavigation, useRouter } from 'expo-router';
import { FlashList } from '@shopify/flash-list';
import { useSignalR } from '@/context/SignalRContext';
import { getMessagesByRoomId, PagedMessageResponse } from '@/apis/chatRoom';
export default function ChatRoomScreen() {
  const { id: roomId } = useLocalSearchParams();
  const { userSession } = useSession();
  const [messages, setMessages] = useState<CreateMessageResponse[]>([]);
  const [newMessage, setNewMessage] = useState('');
  const [isConnecting, setIsConnecting] = useState(false);
  const [isLoadingOlderMessages, setIsLoadingOlderMessages] = useState(false);
  const [pagedMetaData, setPagedMetaData] = useState<PagedMessageResponse>();
  const [pageNumber, setPageNumber] = useState(1);
  const { signalRService, isConnected, onlineUsers, chatRooms } = useSignalR();
  const router = useRouter();
  const navigation = useNavigation();

  useEffect(() => {
    if (!userSession?.token || !roomId || !signalRService) return;
    
    const validRoomId = Array.isArray(roomId) ? roomId[0] : roomId;
    getMessagesByRoomId({
      roomId: validRoomId,
      page: 1,
    }, userSession.token).then((response) => {
      setPagedMetaData(response);
      setMessages(response.items);
      setPageNumber(2); // Set to 2 so next load fetches the next page
    });

    let chatRoom = chatRooms.find(n => n.id === roomId);
    
    if (chatRoom) {
      navigation.setOptions({ title: chatRoom.name, headerBackTitle: 'Chat' });
    }

    // Set up message handler
    signalRService.sendMessageToRoomHandler((message: CreateMessageResponse) => {
        setMessages(prev => [message, ...prev]);
      });
    signalRService.joinRoomHandler((message: string) => {
      console.log(message);
    });
      //signalRService.joinRoom(roomId as string);
  }, [userSession?.token, roomId, signalRService, isConnected, onlineUsers, router, navigation]);

  const loadOlderMessages = async () => {
    if (isLoadingOlderMessages || !pagedMetaData?.hasNext || !signalRService || !userSession) return;
    
    setIsLoadingOlderMessages(true);
    try {
      const validRoomId = Array.isArray(roomId) ? roomId[0] : roomId;
      const response = await getMessagesByRoomId({
        roomId: validRoomId,
        page: pageNumber,
        // Add pagination or offset if your API supports it, e.g. offset: messages.length
      }, userSession.token);
      setMessages(prev => [...prev, ...response.items ]);
      setPagedMetaData(response);

      if (pageNumber < response.totalPagesCount) {
        setPageNumber(prev => prev + 1);
      }
    } catch (error) {
      console.error('Failed to load older messages:', error);
    } finally {
      setIsLoadingOlderMessages(false);
    }
  };

  const handleEndReached = async () => {
    if (!isLoadingOlderMessages && pagedMetaData?.hasNext) {
      await loadOlderMessages();
    }
  };

  const handleSendMessage = async () => {
    if (!signalRService || !userSession?.user || !newMessage.trim()) return;

    const messageToSend = newMessage.trim();
    setNewMessage(''); // <-- Clear input immediately

    try {
      const messageRequest: CreateMessageRequest = {
        userId: userSession.user.id,
        roomId: roomId as string,
        content: messageToSend
      };

      await signalRService.sendMessageToRoom(messageRequest);
    } catch (error) {
      console.error('Failed to send message:', error);
      // Optionally restore the message if sending fails
      setNewMessage(messageToSend);
    }
  };

  if (isConnecting) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color="#0000ff" />
        <Text style={styles.loadingText}>Connecting to chat...</Text>
      </View>
    );
  }

  return (
    <KeyboardAvoidingView 
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      keyboardVerticalOffset={Platform.OS === 'ios' ? 90 : 0}
    >
      <FlashList
        estimatedItemSize={100}
        contentContainerStyle={styles.messageList}
        data={messages}
        inverted={true}
        keyExtractor={(item: CreateMessageResponse) => item.id}
        onEndReached={handleEndReached}
        onEndReachedThreshold={0.1}
        ListFooterComponent={
          isLoadingOlderMessages ? (
            <View style={styles.loadingFooter}>
              <ActivityIndicator size="small" color="#007AFF" />
              <Text style={styles.loadingFooterText}>Loading older messages...</Text>
            </View>
          ) : null
        }
        renderItem={({ item }: { item: CreateMessageResponse }) => {
          const isSent = item.username === userSession?.user?.email;
          return (
            <View style={{ alignItems: isSent ? 'flex-end' : 'flex-start', marginBottom: 8 }}>
              <Text style={styles.senderName}>{item.username}</Text>
              <View style={[
                styles.messageContainer,
                isSent ? styles.sentMessage : styles.receivedMessage
              ]}>
                <Text style={styles.messageText}>{item.content}</Text>
                <Text style={styles.timestamp}>
                  {new Date(item.created).toLocaleTimeString()}
                </Text>
              </View>
            </View>
          );
        }}
        ListEmptyComponent={
          <View style={styles.emptyContainer}>
            <Text style={styles.emptyText}>No messages yet</Text>
          </View>
        }
        showsVerticalScrollIndicator={true}
      />

      <View style={styles.inputContainer}>
        <TextInput
          style={styles.input}
          value={newMessage}
          onChangeText={setNewMessage}
          placeholder="Type a message..."
          multiline
        />
        <TouchableOpacity 
          style={styles.sendButton}
          onPress={handleSendMessage}
          disabled={!newMessage.trim()}
        >
          <Text style={[
            styles.sendButtonText,
            !newMessage.trim() && styles.sendButtonDisabled
          ]}>Send</Text>
        </TouchableOpacity>
      </View>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#fff',
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#fff',
  },
  loadingText: {
    marginTop: 10,
    fontSize: 16,
    color: '#666',
  },
  header: {
    padding: 20,
    borderBottomWidth: 1,
    borderBottomColor: '#eee',
  },
  title: {
    fontSize: 24,
    fontWeight: 'bold',
  },
  messageList: {
    padding: 10,
  },
  messageContainer: {
    maxWidth: '80%',
    padding: 10,
    borderRadius: 10,
    marginBottom: 8,
  },
  sentMessage: {
    alignSelf: 'flex-end',
    backgroundColor: '#007AFF',
  },
  receivedMessage: {
    alignSelf: 'flex-start',
    backgroundColor: '#E5E5EA',
  },
  senderName: {
    fontSize: 12,
    color: '#666',
    marginBottom: 4,
  },
  messageText: {
    fontSize: 16,
    color: '#000',
  },
  timestamp: {
    fontSize: 10,
    color: '#666',
    alignSelf: 'flex-end',
    marginTop: 4,
  },
  inputContainer: {
    flexDirection: 'row',
    padding: 10,
    borderTopWidth: 1,
    borderTopColor: '#eee',
    backgroundColor: '#fff',
  },
  input: {
    flex: 1,
    backgroundColor: '#f0f0f0',
    borderRadius: 20,
    paddingHorizontal: 15,
    paddingVertical: 8,
    marginRight: 10,
    maxHeight: 100,
  },
  sendButton: {
    justifyContent: 'center',
    alignItems: 'center',
    paddingHorizontal: 15,
  },
  sendButtonText: {
    color: '#007AFF',
    fontSize: 16,
    fontWeight: '600',
  },
  sendButtonDisabled: {
    color: '#999',
  },
  emptyContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 20,
  },
  emptyText: {
    fontSize: 16,
    color: '#666',
  },
  loadingFooter: {
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    padding: 20,
  },
  loadingFooterText: {
    marginLeft: 10,
    fontSize: 14,
    color: '#666',
  },
}); 