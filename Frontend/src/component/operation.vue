<template>
	<td>{{ operation.date }}</td>
	<td>{{ amountFormat }}</td>
	<td>{{ operation.kind }}</td>
	<td>{{ operation.category }}</td>
	<td v-if="operation.asset">{{ operation.asset }}</td>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { CurrencyDto, OperationDto, StateDto } from '@/api/state';
import { Prop } from 'vue-property-decorator';
import { State } from 'vuex-class';

@Options({
	name: 'Operation',
})
export default class Operation extends Vue {
	@State('activeState')
	activeState!: StateDto;

	@Prop()
	operation!: OperationDto;

	get amountFormat() {
		const accountCurrency = this.activeState.currencies?.find((c) => c.id == this.operation.currency) as CurrencyDto;
		const format = accountCurrency.format;
		return format?.replace('{0}', (this.operation.amount ?? 0).toString());
	}
}
</script>
