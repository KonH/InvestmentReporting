<template>
	<td>{{ displayDate }}</td>
	<td>{{ operation.kind }}</td>
	<td>{{ operation.category }}</td>
	<td>{{ operation.brokerName }}</td>
	<td>{{ operation.accountName }}</td>
	<td>{{ operation.assetIsin }}</td>
	<td><money :currency-id="operation.currency" :value="operation.amount" /></td>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import OperationData from '@/dto/operationData';
import { Prop } from 'vue-property-decorator';
import Money from '@/component/money.vue';

@Options({
	name: 'Operation',
	components: {
		Money,
	},
})
export default class Operation extends Vue {
	@Prop()
	operation!: OperationData;

	get displayDate() {
		return new Date(this.operation.date ?? '').toUTCString();
	}
}
</script>
