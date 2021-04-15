<template>
	<div class="card" style="width: 8rem">
		<div class="card-body">
			<h4>{{ currency.code }}</h4>
		</div>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { CurrencyDto } from '@/api/state';
import { Prop } from 'vue-property-decorator';

@Options({
	name: 'CurrencyCard',
})
export default class CurrencyCard extends Vue {
	@Prop()
	currency!: CurrencyDto;
}
</script>
