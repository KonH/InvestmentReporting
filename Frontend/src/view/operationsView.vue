<template>
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
			<tr v-for="operation in operations" :key="operation.date + operation.kind + operation.category">
				<operation :operation="operation" />
			</tr>
		</tbody>
	</table>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import Operation from '@/component/operation.vue';
import Backend from '@/service/backend';
import { OperationDto, StateDto } from '@/api/state';
import OperationData from '@/dto/operationData';
import { State } from 'vuex-class';

@Options({
	name: 'OperationsView',
	components: {
		Operation,
	},
})
export default class OperationsView extends Vue {
	@State('activeState')
	activeState!: StateDto;

	allOperations: OperationDto[] = [];

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
		return this.allOperations.map(this.createView);
	}

	createView(dto: OperationDto): OperationData {
		return {
			date: dto.date,
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
}
</script>
