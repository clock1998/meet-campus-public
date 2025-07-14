import { CreateMessageResponse } from "@/services/signalRService";
import { ApiError, baseApiUrl, getOptions, postOptions } from "./base";

// Define the message response type based on the backend GetAllByRoomIdHandlerMessageResponse

// Define the paged response type based on the backend PagedList
export interface PagedMessageResponse {
  items: CreateMessageResponse[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPagesCount: number;
  hasPrevious: boolean;
  hasNext: boolean;
  columnNames: string[];
}

// Define the query parameters interface
export interface GetMessagesByRoomIdParams {
  roomId: string;
  search?: string;
  sortColum?: string;
  sortOrder?: string;
  page?: number;
  pageSize?: number;
}

export async function getMessagesByRoomId(params: GetMessagesByRoomIdParams, token: string): Promise<PagedMessageResponse> {
    const queryParams = new URLSearchParams();
    queryParams.append('roomId', params.roomId);
    
    if (params.search) queryParams.append('Search', params.search);
    if (params.sortColum) queryParams.append('SortColum', params.sortColum);
    if (params.sortOrder) queryParams.append('SortOrder', params.sortOrder);
    if (params.page) queryParams.append('Page', params.page.toString());
    if (params.pageSize) queryParams.append('PageSize', params.pageSize.toString());

    const authOptions = {
        ...getOptions,
        headers: {
            ...getOptions.headers,
            'Authorization': `Bearer ${token}`
        }
    };

    const response = await fetch(`${baseApiUrl}/Message/GetByRoomId?${queryParams.toString()}`, authOptions);
    if (!response.ok) {
        const errorData: ApiError = await response.json(); // if the API returns JSON error info
        throw errorData;
    }
    return await response.json();
}
