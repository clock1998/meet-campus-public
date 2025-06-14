import { User, useSession } from '@/context/AuthContext';
import { CreateMessageResponse, CreateMessageRequest } from '@/services/signalRService';
import React, { useEffect, useState, useRef } from 'react';
import { View, Text, StyleSheet, TextInput, TouchableOpacity, KeyboardAvoidingView, Platform, ActivityIndicator } from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { FlashList } from '@shopify/flash-list';
import { useSignalR } from '@/context/SignalRContext';

interface Message {
  id: string;
  content: string;
  username: string;
  created: Date;
  updated: Date;
}

export default function ChatRoomScreen() {
  const { id: roomId } = useLocalSearchParams();
  const { userSession } = useSession();
  const [messages, setMessages] = useState<Message[]>([]);
  const [newMessage, setNewMessage] = useState('');
  const [isConnecting, setIsConnecting] = useState(false);
  const { signalRService, isConnected, onlineUsers  } = useSignalR();
  const router = useRouter();

  useEffect(() => {
    if (!userSession?.token || !roomId || !signalRService) return;

      // Set up message handler
      signalRService.sendMessageToRoomHandler((message: CreateMessageResponse) => {
        setMessages(prev => [...prev, {
          id: message.id,
          username: message.username,
          content: message.content,
          created: message.created!,
          updated: message.updated!
        }]);
      });

      signalRService.joinRoom(roomId as string).then(() => {
        console.log('Joined room');
      });
  }, [userSession?.token, roomId,signalRService, isConnected, onlineUsers, router]);

  const handleSendMessage = async () => {
    if (!signalRService || !userSession?.user || !newMessage.trim()) return;

    try {
      const messageRequest: CreateMessageRequest = {
        userId: userSession.user.id,
        roomId: roomId as string,
        content: newMessage.trim()
      };

      await signalRService.sendMessageToRoom(messageRequest);
      setNewMessage('');
    } catch (error) {
      console.error('Failed to send message:', error);
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
      <View style={styles.header}>
        <Text style={styles.title}>Chat Room</Text>
      </View>

      <FlashList
        estimatedItemSize={100}
        style={styles.messageList}
        data={messages}
        keyExtractor={(item: Message) => item.id}
        renderItem={({ item }: { item: Message }) => (
          <View style={[
            styles.messageContainer,
            item.username === userSession?.user?.email ? styles.sentMessage : styles.receivedMessage
          ]}>
            <Text style={styles.senderName}>{item.username}</Text>
            <Text style={styles.messageText}>{item.content}</Text>
            <Text style={styles.timestamp}>
              {new Date(item.created).toLocaleTimeString()}
            </Text>
          </View>
        )}
        ListEmptyComponent={
          <View style={styles.emptyContainer}>
            <Text style={styles.emptyText}>No messages yet</Text>
          </View>
        }
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
    flex: 1,
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
}); 