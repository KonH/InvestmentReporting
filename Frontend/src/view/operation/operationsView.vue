<template>
	<div class="form-group">
		<label>
			Start date:
			<input ref="startDate" type="date" class="form-control" @change="onStartDateChange" />
		</label>
		<label class="ml-3">
			End date:
			<input ref="endDate" type="date" class="form-control" @change="onEndDateChange" />
		</label>
		<label class="ml-3">
			Category:
			<select ref="category" class="form-control" @change="onCategoryChange">
				<option v-for="category in categories" :key="category" :value="category">
					{{ category }}
				</option>
			</select>
		</label>
		<label class="ml-3">
			Broker:
			<select ref="broker" class="form-control" @change="onBrokerChange">
				<option v-for="broker in brokers" :key="broker.id" :value="broker.id">
					{{ broker.displayName }}
				</option>
			</select>
		</label>
		<label class="ml-3">
			Account:
			<select ref="account" class="form-control" @change="onAccountChange">
				<option v-for="account in accounts" :key="account.id" :value="account.id">
					{{ account.displayName }}
				</option>
			</select>
		</label>
		<asset-selector :value="targetAsset" emit-isin="true" class="ml-3" @input="onAssetChange" />
	</div>
	<div>
		<label>
			<b>Total:</b>&nbsp;
			<template v-for="currencyCode in currencyCodes" :key="currencyCode">
				<money :value="getCurrencySum(currencyCode)" :currency-code="currencyCode" />&nbsp;
			</template>
		</label>
	</div>
	<table class="table table-sm table-striped">
		<thead>
			<tr>
				<th scope="col">Date</th>
				<th scope="col">Kind</th>
				<th scope="col">Category</th>
				<th scope="col">Broker</th>
				<th scope="col">Account</th>
				<th scope="col">Asset</th>
				<th scope="col">Amount</th>
			</tr>
		</thead>
		<tbody>
			<tr v-for="operation in operations" :key="operation.id">
				<operation :operation="operation" />
			</tr>
		</tbody>
	</table>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import Operation from '@/component/operation/operation.vue';
import Backend from '@/service/backend';
import { AccountDto, BrokerDto, OperationDto, StateDto } from '@/api/state';
import OperationData from '@/dto/operationData';
import { State } from 'vuex-class';
import { Ref } from 'vue-property-decorator';
import AssetSelector from '@/component/common/assetSelector.vue';
import Money from '@/component/common/money.vue';

@Options({
	name: 'OperationsView',
	components: {
		Operation,
		AssetSelector,
		Money,
	},
})
export default class OperationsView extends Vue {
	@State('activeState')
	activeState!: StateDto;

	@Ref('startDate')
	startDateInput!: HTMLInputElement;

	@Ref('endDate')
	endDateInput!: HTMLInputElement;

	@Ref('category')
	categoryInput!: HTMLSelectElement;

	@Ref('broker')
	brokerInput!: HTMLSelectElement;

	@Ref('account')
	accountInput!: HTMLSelectElement;

	allOperations: OperationDto[] = [];

	startDateValue = new Date(1, 1, 1);
	endDateValue = new Date(Number.MAX_VALUE, 1, 1);
	targetCategory = '';
	targetBroker = '';
	targetAccount = '';
	targetAsset = '';

	async created() {
		const startDate = new Date(1, 1, 1).toISOString();
		const endDate = new Date().toISOString();
		const response = await Backend.state().operation.operationList({
			startDate: startDate,
			endDate: endDate,
		});
		this.allOperations = response.data;
	}

	get operations() {
		return this.allOperations.filter(this.preFilter).map(this.createView).filter(this.postFilter);
	}

	preFilter(dto: OperationDto) {
		if (this.targetCategory) {
			if (dto.category != this.targetCategory) {
				return false;
			}
		}
		if (this.targetBroker) {
			if (this.targetAccount) {
				return dto.account == this.targetAccount;
			}
			return dto.broker == this.targetBroker;
		}
		return true;
	}

	postFilter(data: OperationData) {
		if (data.date) {
			if (data.date < this.startDateValue) {
				return false;
			}
			if (data.date > this.endDateValue) {
				return false;
			}
		}
		if (this.targetAsset) {
			return data.assetIsin == this.targetAsset;
		}
		return true;
	}

	createView(dto: OperationDto): OperationData {
		const id = (dto.date ?? '') + (dto.category ?? '') + (dto.broker ?? '') + (dto.account ?? '') + (dto.asset ?? '') + (Math.random() * 1000).toString();
		return {
			id: id,
			date: new Date(dto.date ?? ''),
			kind: dto.kind,
			currency: dto.currency,
			amount: dto.amount,
			category: dto.category,
			brokerName: this.getBrokerName(dto) ?? 'N/A',
			accountName: this.getAccountName(dto) ?? 'N/A',
			assetIsin: this.getAssetIsin(dto) ?? 'N/A',
		};
	}

	getBroker(dto: OperationDto) {
		return this.activeState.brokers?.find((b) => b.id == dto.broker);
	}

	getBrokerName(dto: OperationDto) {
		return this.getBroker(dto)?.displayName;
	}

	getAccountName(dto: OperationDto) {
		return this.getBroker(dto)?.accounts?.find((a) => a.id == dto.account)?.displayName;
	}

	getAssetIsin(dto: OperationDto) {
		return this.getBroker(dto)?.inventory?.find((a) => a.id == dto.asset)?.isin;
	}

	get brokers() {
		const empty: BrokerDto[] = [{ displayName: '' }];
		const brokers = this.activeState.brokers ?? [];
		return empty.concat(brokers);
	}

	get accounts() {
		const empty: AccountDto[] = [{ displayName: '' }];
		const broker = this.activeState.brokers?.find((b) => b.id == this.targetBroker);
		const accounts = broker?.accounts ?? [];
		return empty.concat(accounts);
	}

	get assets() {
		const empty = [''];
		const assetIsins = this.allOperations.map((dto) => this.createView(dto).assetIsin).filter((v) => v != 'N/A');
		return new Set(empty.concat(assetIsins));
	}

	get categories() {
		const empty = [''];
		const categories = this.allOperations.map((dto) => this.createView(dto).category ?? '');
		return new Set(empty.concat(categories));
	}

	onStartDateChange() {
		this.startDateValue = new Date(this.startDateInput.value);
	}

	onEndDateChange() {
		this.endDateValue = new Date(this.endDateInput.value);
	}

	onCategoryChange() {
		this.targetCategory = this.categoryInput.value;
	}

	onBrokerChange() {
		this.targetBroker = this.brokerInput.value;
		this.targetAccount = '';
	}

	onAccountChange() {
		this.targetAccount = this.accountInput.value;
	}

	onAssetChange(value: string) {
		this.targetAsset = value;
	}

	get currencyCodes() {
		return this.activeState.currencies?.map((c) => c.code);
	}

	getCurrencySum(code: string) {
		const currencyOps = this.operations.filter((o) => o.currency == code);
		return currencyOps.map((o) => o.amount ?? 0).reduce((p, c) => p + c, 0);
	}
}
</script>
