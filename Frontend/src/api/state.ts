/* eslint-disable */
/* tslint:disable */
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

export interface OperationDto {
  /** @format date-time */
  date?: string;
  kind?: string | null;
  currency?: string | null;

  /** @format double */
  amount?: number;
  category?: string | null;
  broker?: string | null;
  account?: string | null;
  asset?: string | null;
}

export interface AccountDto {
  id?: string | null;
  currency?: string | null;
  displayName?: string | null;

  /** @format double */
  balance?: number;
}

export interface AssetDto {
  id?: string | null;
  isin?: string | null;

  /** @format int32 */
  count?: number;
}

export interface BrokerDto {
  id?: string | null;
  displayName?: string | null;
  accounts?: AccountDto[] | null;
  inventory?: AssetDto[] | null;
}

export interface CurrencyDto {
  code?: string | null;
  format?: string | null;
}

export interface StateDto {
  brokers?: BrokerDto[] | null;
  currencies?: CurrencyDto[] | null;
}

export type QueryParamsType = Record<string | number, any>;
export type ResponseFormat = keyof Omit<Body, "body" | "bodyUsed">;

export interface FullRequestParams extends Omit<RequestInit, "body"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseFormat;
  /** request body */
  body?: unknown;
  /** base url */
  baseUrl?: string;
  /** request cancellation token */
  cancelToken?: CancelToken;
}

export type RequestParams = Omit<FullRequestParams, "body" | "method" | "query" | "path">;

export interface ApiConfig<SecurityDataType = unknown> {
  baseUrl?: string;
  baseApiParams?: Omit<RequestParams, "baseUrl" | "cancelToken" | "signal">;
  securityWorker?: (securityData: SecurityDataType | null) => Promise<RequestParams | void> | RequestParams | void;
}

export interface HttpResponse<D extends unknown, E extends unknown = unknown> extends Response {
  data: D;
  error: E;
}

type CancelToken = Symbol | string | number;

export enum ContentType {
  Json = "application/json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
}

export class HttpClient<SecurityDataType = unknown> {
  public baseUrl: string = "";
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private abortControllers = new Map<CancelToken, AbortController>();

  private baseApiParams: RequestParams = {
    credentials: "same-origin",
    headers: {},
    redirect: "follow",
    referrerPolicy: "no-referrer",
  };

  constructor(apiConfig: ApiConfig<SecurityDataType> = {}) {
    Object.assign(this, apiConfig);
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  private addQueryParam(query: QueryParamsType, key: string) {
    const value = query[key];

    return (
      encodeURIComponent(key) +
      "=" +
      encodeURIComponent(Array.isArray(value) ? value.join(",") : typeof value === "number" ? value : `${value}`)
    );
  }

  protected toQueryString(rawQuery?: QueryParamsType): string {
    const query = rawQuery || {};
    const keys = Object.keys(query).filter((key) => "undefined" !== typeof query[key]);
    return keys
      .map((key) =>
        typeof query[key] === "object" && !Array.isArray(query[key])
          ? this.toQueryString(query[key] as QueryParamsType)
          : this.addQueryParam(query, key),
      )
      .join("&");
  }

  protected addQueryParams(rawQuery?: QueryParamsType): string {
    const queryString = this.toQueryString(rawQuery);
    return queryString ? `?${queryString}` : "";
  }

  private contentFormatters: Record<ContentType, (input: any) => any> = {
    [ContentType.Json]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string") ? JSON.stringify(input) : input,
    [ContentType.FormData]: (input: any) =>
      Object.keys(input || {}).reduce((data, key) => {
        data.append(key, input[key]);
        return data;
      }, new FormData()),
    [ContentType.UrlEncoded]: (input: any) => this.toQueryString(input),
  };

  private mergeRequestParams(params1: RequestParams, params2?: RequestParams): RequestParams {
    return {
      ...this.baseApiParams,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...(this.baseApiParams.headers || {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  private createAbortSignal = (cancelToken: CancelToken): AbortSignal | undefined => {
    if (this.abortControllers.has(cancelToken)) {
      const abortController = this.abortControllers.get(cancelToken);
      if (abortController) {
        return abortController.signal;
      }
      return void 0;
    }

    const abortController = new AbortController();
    this.abortControllers.set(cancelToken, abortController);
    return abortController.signal;
  };

  public abortRequest = (cancelToken: CancelToken) => {
    const abortController = this.abortControllers.get(cancelToken);

    if (abortController) {
      abortController.abort();
      this.abortControllers.delete(cancelToken);
    }
  };

  public request = async <T = any, E = any>({
    body,
    secure,
    path,
    type,
    query,
    format = "json",
    baseUrl,
    cancelToken,
    ...params
  }: FullRequestParams): Promise<HttpResponse<T, E>> => {
    const secureParams = (secure && this.securityWorker && (await this.securityWorker(this.securityData))) || {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const queryString = query && this.toQueryString(query);
    const payloadFormatter = this.contentFormatters[type || ContentType.Json];

    return fetch(`${baseUrl || this.baseUrl || ""}${path}${queryString ? `?${queryString}` : ""}`, {
      ...requestParams,
      headers: {
        ...(type && type !== ContentType.FormData ? { "Content-Type": type } : {}),
        ...(requestParams.headers || {}),
      },
      signal: cancelToken ? this.createAbortSignal(cancelToken) : void 0,
      body: typeof body === "undefined" || body === null ? null : payloadFormatter(body),
    }).then(async (response) => {
      const r = response as HttpResponse<T, E>;
      r.data = (null as unknown) as T;
      r.error = (null as unknown) as E;

      const data = await response[format]()
        .then((data) => {
          if (r.ok) {
            r.data = data;
          } else {
            r.error = data;
          }
          return r;
        })
        .catch((e) => {
          r.error = e;
          return r;
        });

      if (cancelToken) {
        this.abortControllers.delete(cancelToken);
      }

      if (!response.ok) throw data;
      return data;
    });
  };
}

/**
 * @title InvestmentReporting.StateService
 * @version v1
 */
export class Api<SecurityDataType extends unknown> extends HttpClient<SecurityDataType> {
  account = {
    /**
     * No description
     *
     * @tags Account
     * @name AccountCreate
     * @request POST:/Account
     */
    accountCreate: (query: { broker: string; currency: string; displayName: string }, params: RequestParams = {}) =>
      this.request<void, void>({
        path: `/Account`,
        method: "POST",
        query: query,
        ...params,
      }),
  };
  asset = {
    /**
     * No description
     *
     * @tags Asset
     * @name BuyAssetCreate
     * @request POST:/Asset/BuyAsset
     */
    buyAssetCreate: (
      query: {
        date: string;
        broker: string;
        payAccount: string;
        feeAccount: string;
        name: string;
        category: string;
        isin: string;
        price: number;
        fee: number;
        count: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, void>({
        path: `/Asset/BuyAsset`,
        method: "POST",
        query: query,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Asset
     * @name SellAssetCreate
     * @request POST:/Asset/SellAsset
     */
    sellAssetCreate: (
      query: {
        date: string;
        broker: string;
        payAccount: string;
        feeAccount: string;
        asset: string;
        price: number;
        fee: number;
        count: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, void>({
        path: `/Asset/SellAsset`,
        method: "POST",
        query: query,
        ...params,
      }),
  };
  broker = {
    /**
     * No description
     *
     * @tags Broker
     * @name BrokerCreate
     * @request POST:/Broker
     */
    brokerCreate: (query: { displayName: string }, params: RequestParams = {}) =>
      this.request<void, void>({
        path: `/Broker`,
        method: "POST",
        query: query,
        ...params,
      }),
  };
  expense = {
    /**
     * No description
     *
     * @tags Expense
     * @name ExpenseCreate
     * @request POST:/Expense
     */
    expenseCreate: (
      query: { date: string; broker: string; account: string; amount: number; category: string; asset?: string | null },
      params: RequestParams = {},
    ) =>
      this.request<void, void>({
        path: `/Expense`,
        method: "POST",
        query: query,
        ...params,
      }),
  };
  income = {
    /**
     * No description
     *
     * @tags Income
     * @name IncomeCreate
     * @request POST:/Income
     */
    incomeCreate: (
      query: { date: string; broker: string; account: string; amount: number; category: string; asset?: string | null },
      params: RequestParams = {},
    ) =>
      this.request<void, void>({
        path: `/Income`,
        method: "POST",
        query: query,
        ...params,
      }),
  };
  operation = {
    /**
     * No description
     *
     * @tags Operation
     * @name OperationList
     * @request GET:/Operation
     */
    operationList: (query: { startDate: string; endDate: string }, params: RequestParams = {}) =>
      this.request<OperationDto[], any>({
        path: `/Operation`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Operation
     * @name OperationDelete
     * @request DELETE:/Operation
     */
    operationDelete: (params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/Operation`,
        method: "DELETE",
        ...params,
      }),
  };
  state = {
    /**
     * No description
     *
     * @tags State
     * @name StateList
     * @request GET:/State
     */
    stateList: (query: { date: string }, params: RequestParams = {}) =>
      this.request<StateDto, any>({
        path: `/State`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags State
     * @name StateDelete
     * @request DELETE:/State
     */
    stateDelete: (params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/State`,
        method: "DELETE",
        ...params,
      }),
  };
}
