<template>
	{{ format }}
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { CurrencyDto, StateDto } from '@/api/state';
import { Prop } from 'vue-property-decorator';
import { State } from 'vuex-class';

@Options({
	name: 'Money',
})
export default class Money extends Vue {
	@State('activeState')
	activeState!: StateDto;

	@Prop()
	value!: number;

	@Prop()
	currencyCode!: string;

	get format() {
		const currency = this.activeState.currencies?.find((c) => c.code == this.currencyCode) as CurrencyDto;
		const format = currency.format;
		const sign = this.value < 0 ? '-' : '';
		const value = Math.abs(this.value).toLocaleString();
		return format?.replace('{sign}', sign)?.replace('{value}', value);
	}
}
</script>
